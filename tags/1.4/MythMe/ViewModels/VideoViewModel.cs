using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace MythMe
{
    public class VideoViewModel : INotifyPropertyChanged
    {
        //people info
        public string name { get; set; }
        public string videoPersonId { get; set; }

        public string intid { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public string plot { get; set; }
        public string category { get; set; }
        public string inetref { get; set; }
        public string homepage { get; set; }
        public string releasedate { get; set; }
        public int season { get; set; }
        public int episode { get; set; }
        public string filename { get; set; }
        public string director { get; set; }
        public string year { get; set; }
        public string rating { get; set; }
        public string length { get; set; }
        public string hash { get; set; }
        public string coverfile { get; set; }
        public string host { get; set; }
        public string insertdate { get; set; }
        public string type { get; set; }


        public string coverart { get; set; }
        public string seasonText { get; set; }
        public string episodeText { get; set; }
        public string fullEpisode { get; set; }
        public string group { get; set; }

        public string fullFilename { get; set; }

        public string alpha { get; set; }

        public Visibility showCoverartList { get; set; }
        public Visibility showCoverartDetails { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
