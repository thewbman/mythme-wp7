using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MythMe
{
    public class ProgramViewModel : INotifyPropertyChanged
    {
        public string title { get; set; }
        public string subtitle { get; set; }
        public string description { get; set; }
        //public string season { get; set; }
        //public string epsiode { get; set; }
        public string category { get; set; }
        public int chanid { get; set; }
        public string channum { get; set; }
        public int channumint { get; set; }
        public string callsign { get; set; }
        public string channame { get; set; }

        public string filename { get; set; }
        public Int64 filesize { get; set; }
        public Int64 starttimeint { get; set; }
        public string starttime { get; set; }
        public string starttimespace { get; set; }
        public Int64 endtimeint { get; set; }
        public string endtime { get; set; }
        public string endtimespace { get; set; }
        public string findid { get; set; }
        public string hostname { get; set; }
        public int sourceid { get; set; }
        public int cardid { get; set; }

        public int inputid { get; set; }
        public int recpriority { get; set; }
        public int recstatus { get; set; }
        public string recstatustext { get; set; }
        public int recordid { get; set; }
        public int rectype { get; set; }
        //public string dupin { get; set; }
        //public string dupmethod { get; set; }
        public Int64 recstarttsint { get; set; }
        public string recstartts { get; set; }

        public Int64 recendtsint { get; set; }
        public string recendts { get; set; }
        //public string programflags { get; set; }
        public string recgroup { get; set; }
        //public string outputfilters { get; set; }
        public string seriesid { get; set; }
        public string programid { get; set; }
        //public string initref { get; set; }
        public string lastmodified { get; set; }
        //public string stars { get; set; }

        public string airdate { get; set; }
        public string playgroup { get; set; }
        public string recpriority2 { get; set; }
        public string parentid { get; set; }
        public string storagegroup { get; set; }
        //public string audio_props { get; set; }
        //public string video_props { get; set; }
        //public string year { get; set; }

        public string screenshot { get; set; }
        public string chanicon { get; set; }

        public string recordedfourthline { get; set; }
        public string upcomingfourthline { get; set; }

        public string recsort { get; set; }
        public string recsortdisplay { get; set; }

        public string guidesort { get; set; }
        public string guidesortdisplay { get; set; }


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
