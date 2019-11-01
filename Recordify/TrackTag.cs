using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recordify
{
    class TrackTag
    {
        public enum status
        {
            UNDEFINED = 0,
            RECORDING,
            CONVERTING,
            FINISHED
        }
        

        #region Eigenschaften
        public string Artist
        {
            get;
            set;
        }
        public string Title
        {
            get;
            set;
        }
        public string Album
        {
            get;
            set;
        }
        public bool IsPlaying
        {
            get;
            set;
        }
        public int Status
        {
            get;
            set;
        }
        #endregion

        #region Konstruktoren
        public TrackTag()
        {
            Artist = "-";
            Title = "-";
            Album = "-";
            IsPlaying = false;
        }

        public TrackTag(bool isPlaying)
        {
            Artist = "-";
            Title = "-";
            Album = "-";
            IsPlaying = isPlaying;
        }

        public TrackTag(string artist, string title, string album, bool isPlaying)
        {
            Artist = artist;
            Title = title;
            Album = album;
            IsPlaying = isPlaying;
        }
        #endregion
    }
}
