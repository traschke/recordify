using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Recordify
{
    class NAudioRecorder
    {
        #region Attribute
        private MMDevice device;
        private string filename;
        private string artist = "";
        private string title = "";
        private string album = "";
        private int bitrate = (int)bitrates.a128;

        private ThreadStart convertThreadStart;
        private Thread convertThread;

        private IWaveIn waveIn;
        private WaveFileWriter waveFileWriter;

        public delegate void EventDelegate();
        public event EventDelegate RecordingStarted;
        public event EventDelegate RecordingStopped;

        public event EventDelegate ToMp3Started;
        public event EventDelegate ToMp3Finished;

        enum bitrates { a128 = 0, a192, a256, a320, aVBR };
        #endregion

        #region Eigenschaften
        public string Filename
        {
            get
            {
                return filename;
            }
        }
        #endregion

        #region Konstruktoren
        public NAudioRecorder(MMDevice device, string folder, string artist, string title, string album, int bitrate)
        {
            this.device = device;
            this.artist = artist;
            this.title = title;
            this.album = album;
            this.bitrate = bitrate;

            this.filename = folder + "\\" + Guid.NewGuid().ToString("N") + ".wav";

            this.waveIn = new WasapiCapture(this.device);
            this.waveFileWriter = new WaveFileWriter(this.filename, this.waveIn.WaveFormat);

            this.waveIn.DataAvailable += waveIn_DataAvailable;
        }
        #endregion

        #region Public Methoden
        public void StartRecording()
        {
            waveIn.StartRecording();
            OnRecordingStarted();
        }

        public void StopRecording()
        {
            waveIn.StopRecording();
            waveIn.Dispose();
            waveFileWriter.Close();
            OnRecordingStopped();
            ConvertWavToMp3Thread();
        }

        public void OnRecordingStarted()
        {
            if (RecordingStarted != null)
                RecordingStarted();
        }

        public void OnRecordingStopped()
        {
            if (RecordingStopped != null)
                RecordingStopped();
        }

        public void OnToMp3Started()
        {
            if (ToMp3Started != null)
                ToMp3Started();
        }

        public void OnToMp3Finished()
        {
            if (ToMp3Finished != null)
                ToMp3Finished();
        }
        #endregion

        #region Private Methoden
        private void ConvertWavToMp3Thread()
        {
            convertThreadStart = new ThreadStart(ConvertWavToMp3);
            convertThread = new Thread(convertThreadStart);
            convertThread.Start();
        }

        //TODO: in Thread ausführen
        private void ConvertWavToMp3()
        {
            string bitrateArg = "";

            switch (bitrate)
            {
                case (int)bitrates.a128:
                    bitrateArg = "-b 128";
                    break;
                case (int)bitrates.a192:
                    bitrateArg = "-b 192";
                    break;
                case (int)bitrates.a256:
                    bitrateArg = "-b 256";
                    break;
                case (int)bitrates.a320:
                    bitrateArg = "-b 320";
                    break;
                case (int)bitrates.aVBR:
                    bitrateArg = "-V2";
                    break;
            }

            if (!File.Exists(filename))
                return;

            //Thread.Sleep(500);
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardOutput = true;
            //process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            //Mp3Tag tag = Util.ExtractMp3Tag(filePath);

            if (artist != null)
            {
                artist = Utils.cleanSpecialCharacters(artist);
            }
            if (title != null)
            {
                title = Utils.cleanSpecialCharacters(title);
            }
            if (album != null)
            {
                album = Utils.cleanSpecialCharacters(album);
            }
            

            string mp3filename = Utils.getFilenameWithCounterPlusExtensionChange(Path.Combine(Path.GetDirectoryName(filename), artist + " - " + title), "mp3");


            process.StartInfo.FileName = "lame.exe";
            process.StartInfo.Arguments = string.Format("{2} --tt \"{3}\" --ta \"{4}\" --tl \"{5}\"  \"{0}\" \"{1}\"",
                filename,
                mp3filename,
                bitrateArg,
                title,
                artist,
                album);

            System.Console.WriteLine(process.StartInfo.Arguments);



            process.StartInfo.WorkingDirectory = new FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).DirectoryName;
            OnToMp3Started();
            process.Start();
            process.WaitForExit();
            OnToMp3Finished();

            if (!process.HasExited)
                process.Kill();
            //TODO: Delete catchen
            bool deleteError;

            do
            {
                try
                {
                    File.Delete(filename);
                    deleteError = false;
                    System.Console.WriteLine("Temp Wav Datei gelöscht!");
                }
                catch (Exception e)
                {
                    deleteError = true;
                    System.Console.WriteLine("Löschen der Temp Wav Datei fehlgeschlagen, versuche es nochmal...");
                }
            } while (deleteError);
        }
        #endregion

        #region Events
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveFileWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            waveFileWriter.Flush();
        }
        #endregion
    }
}
