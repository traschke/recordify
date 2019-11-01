using JariZ;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

namespace Recordify
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attribute
        private SpotifyChecker checker;
        private VistaFolderBrowserDialog vistaFolderBrowserDialog;
        private NAudioRecorder nAudioRecorder;
        private ObservableCollection<TrackTag> TrackTags;
        #endregion

        #region Konstruktor
        public MainWindow()
        {
            InitializeComponent();
            checker = new SpotifyChecker();
            checker.TrackChanged += checker_TrackChanged;
            checker.TrackStarted += checker_TrackStarted;
            checker.TrackPaused += checker_TrackPaused;

            vistaFolderBrowserDialog = new VistaFolderBrowserDialog();
            vistaFolderBrowserDialog.Description = "Please select a folder";
            vistaFolderBrowserDialog.UseDescriptionForTitle = true;

            vistaFolderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Recordify";
            txbFolder.Text = vistaFolderBrowserDialog.SelectedPath;

            string[] descBitrates = { "128 kBit/s", "192 kBit/s", "256 kBit/s", "320 kBit/s", "Variable Bitrate" };
            cmbBitrate.ItemsSource = descBitrates;
            cmbBitrate.SelectedIndex = 0;

            LoadRecordingDevicesToCombobox();

            TrackTags = new ObservableCollection<TrackTag>();
            lvHistory.ItemsSource = TrackTags;
        }
        #endregion

        #region Events
        //TODO: Beim starten eines neuen Titels im Pausemodus, starten TrackChanged und TrackStarted die aufnahme!
        void checker_TrackChanged()
        {
            preStartRecording();
        }

        void checker_TrackPaused()
        {
            StopRecording();
        }

        void checker_TrackStarted()
        {
            preStartRecording();
        }

        void nAudioRecorder_RecordingStopped()
        {
            Dispatcher.BeginInvoke(new Action(() => lblRecordingStatus.Foreground = new SolidColorBrush(Colors.Gray)));
        }

        void nAudioRecorder_RecordingStarted()
        {
            Dispatcher.BeginInvoke(new Action(() => lblRecordingStatus.Foreground = new SolidColorBrush(Colors.Red)));
        }
        #endregion


        #region Hilfsmethoden
        private void preStartRecording()
        {
            if (checker.Trackinfos.IsPlaying)
            {
                
                    Dispatcher.BeginInvoke(new Action(() => lblArtist.Content = checker.Trackinfos.Artist));
                    Dispatcher.BeginInvoke(new Action(() => lblTitle.Content = checker.Trackinfos.Title));
                    Dispatcher.BeginInvoke(new Action(() => lblAlbum.Content = checker.Trackinfos.Album));
                    //Dispatcher.BeginInvoke(new Action(() => lbHistory.Items.Insert(0, checker.Trackinfos.Artist + " - " + checker.Trackinfos.Title)));
                    Dispatcher.BeginInvoke(new Action(() => TrackTags.Insert(0, checker.Trackinfos)));
                    Dispatcher.BeginInvoke(new Action(() => StartRecording((MMDevice)cmbDevices.SelectedItem, checker.Trackinfos.Artist, checker.Trackinfos.Title, checker.Trackinfos.Album)));
            }
        }

        private void StartRecording(MMDevice device, string artist, string title, string album)
        {
            if (device != null)
            {
                if (nAudioRecorder != null)
                {
                    StopRecording();
                }
                //Auf Werbung prüfen
                if (!checker.Trackinfos.Album.StartsWith("http://") || !checker.Trackinfos.Album.StartsWith("spotify"))
                {
                    //TODO: Vorher prüfen, ob Pfad korrekt ist und Ordner existiert, ggf. erstellen
                    nAudioRecorder = new NAudioRecorder(device, txbFolder.Text, artist, title, album, cmbBitrate.SelectedIndex);
                    nAudioRecorder.RecordingStarted += nAudioRecorder_RecordingStarted;
                    nAudioRecorder.RecordingStopped += nAudioRecorder_RecordingStopped;
                    nAudioRecorder.StartRecording();
                }
            }
        }

        private void StopRecording()
        {
            if (nAudioRecorder != null)
            {
                string temp = nAudioRecorder.Filename;
                nAudioRecorder.StopRecording();
                nAudioRecorder = null;
            }
        }

        private void LoadRecordingDevicesToCombobox()
        {
            MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
            List<MMDevice> devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
            cmbDevices.ItemsSource = devices;
            cmbDevices.SelectedIndex = 0;
        }
        #endregion

        #region Click-Events
        private void btnStart_Click_1(object sender, RoutedEventArgs e)
        {
            checker.startScanning();
            btnStop.IsEnabled = true;
            btnStart.IsEnabled = false;
            gbRecordingSettings.IsEnabled = false;
        }

        private void btnStop_Click_1(object sender, RoutedEventArgs e)
        {
            checker.stopScanning();
            btnStop.IsEnabled = false;
            btnStart.IsEnabled = true;
            gbRecordingSettings.IsEnabled = true;
            StopRecording();
        }

        private void btnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            if (VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            {
                if ((bool)vistaFolderBrowserDialog.ShowDialog(this))
                {
                    txbFolder.Text = vistaFolderBrowserDialog.SelectedPath;
                }
            }
        }

        private void btnOpenFolder_Click_1(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(txbFolder.Text))
                Process.Start(txbFolder.Text);
        }
        #endregion

        #region Window-Events
        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            checker.stopScanning();
            if (nAudioRecorder != null)
            {
                nAudioRecorder.StopRecording();
            }
        }
        #endregion
    }
}
