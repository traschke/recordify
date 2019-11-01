using JariZ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recordify
{
    class SpotifyChecker
    {
        #region Attribute
        private SpotifyAPI spotifyApi;
        private Responses.CFID cfid;
        private Responses.Status currentStatus;

        private ThreadStart scanningThreadStart;
        private Thread scanningThread;

        private bool askStopScanning = false;

        public delegate void EventDelegate();
        public event EventDelegate TrackChanged;
        public event EventDelegate TrackStarted;
        public event EventDelegate TrackPaused;

        private TrackTag trackInfos;
        private TrackTag oldTrackInfos;
        #endregion

        #region Eigenschaften
        public TrackTag Trackinfos
        {
            get
            {
                return trackInfos;
            }
        }
        #endregion

        #region Konstruktoren
        public SpotifyChecker()
        {
            spotifyApi = new SpotifyAPI(SpotifyAPI.GetOAuth(), "recordify.spotilocal.com");
            cfid = spotifyApi.CFID;
            currentStatus = spotifyApi.Status;
            scanningThreadStart = new ThreadStart(getData);
        }
        #endregion

        #region Public Methoden
        public void startScanning()
        {
            scanningThread = new Thread(scanningThreadStart);
            askStopScanning = false;
            scanningThread.Start();
        }

        public void stopScanning()
        {
            askStopScanning = true;
        }

        public void OnTrackChanged()
        {
            if (TrackChanged != null)
                TrackChanged();
        }

        public void OnTrackStarted()
        {
            if (TrackStarted != null)
                TrackStarted();
        }

        public void OnTrackPaused()
        {
            if (TrackPaused != null)
                TrackPaused();
        }
        #endregion

        #region Private Methoden
        private void getData()
        {
            while (!askStopScanning)
            {
                cfid = spotifyApi.CFID;
                currentStatus = spotifyApi.Status;

                //Console.Out.WriteLine("Album Art URL: " + spotifyApi.getArt(currentStatus.track.album_resource.uri));
                if (Process.GetProcessesByName("spotify").Length > 0)
                {
                    // Prüfen, ob schon ein Track abgespielt wird, wenn ja als alten Track abspeichern
                    if (trackInfos != null)
                    {
                        oldTrackInfos = trackInfos;
                    }

                    // Prüfen, ob Spotify gerade einen Track abspielt
                    if (currentStatus.playing)
                    {
                        if (!currentStatus.track.album_resource.name.StartsWith("http://"))
                        {
                            trackInfos = new TrackTag(currentStatus.track.artist_resource.name, currentStatus.track.track_resource.name, currentStatus.track.album_resource.name, true);
                        }
                        else
                        {
                            trackInfos = new TrackTag(currentStatus.track.artist_resource.name, currentStatus.track.track_resource.name, currentStatus.track.album_resource.name, false);
                        }
                    }
                    else
                    {
                        trackInfos = new TrackTag(currentStatus.track.artist_resource.name, currentStatus.track.track_resource.name, currentStatus.track.album_resource.name, false);
                    }

                    // Prüfen, ob sich der Track verändert hat!
                    if (oldTrackInfos != null)
                    {
                        if (oldTrackInfos.Artist != trackInfos.Artist || oldTrackInfos.Title != trackInfos.Title || oldTrackInfos.Album != trackInfos.Album)
                        {
                            OnTrackChanged();
                            Console.WriteLine("Track changed to " + trackInfos.Artist + " - " + trackInfos.Title + "!");
                        }

                        if (oldTrackInfos.IsPlaying != trackInfos.IsPlaying)
                        {
                            if (trackInfos.IsPlaying)
                            {
                                OnTrackStarted();
                                Console.WriteLine("Playing started!");
                            }
                            else
                            {
                                OnTrackPaused();
                                Console.WriteLine("Playing paused!");
                            }
                        }
                    }
                    else
                    {
                        OnTrackChanged();
                        Console.WriteLine("Track changed to " + trackInfos.Artist + " - " + trackInfos.Title + "!");
                    }
                }

                #region old
                //if (currentStatus.playing)
                //{
                //    if (trackInfos != null)
                //    {
                //        oldTrackInfos = trackInfos;
                //    }

                //    if (currentStatus.track != null)
                //    {
                //        trackInfos = new TrackTag(currentStatus.track.artist_resource.name, currentStatus.track.track_resource.name, currentStatus.track.album_resource.name);
                //    }
                //    else
                //    {
                //        trackInfos = new TrackTag();
                //    }

                //    if (oldTrackInfos != null)
                //    {
                //        if (trackInfos.Artist != oldTrackInfos.Artist && trackInfos.Title != oldTrackInfos.Title)
                //        {
                //            OnEvent();
                //            Console.WriteLine("Track changed to " + trackInfos.Artist + " - " + trackInfos.Title + "!");
                //        }
                //    }
                //    else
                //    {
                //        OnEvent();
                //        Console.WriteLine("Track changed to " + trackInfos.Artist + " - " + trackInfos.Title + "!");
                //    }
                //}
                //else
                //{
                //    trackInfos = new TrackTag();
                //    OnEvent();
                //}
                #endregion

                Thread.Sleep(50);
            }

        }
        #endregion
    }
}
