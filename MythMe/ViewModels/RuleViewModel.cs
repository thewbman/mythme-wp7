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
    public class RuleViewModel : INotifyPropertyChanged
    {

        public int recordid { get; set; }
        public int type { get; set; }
        public string chanid { get; set; }
        public string starttime { get; set; }
        public string startdate { get; set; }
        public string endtime { get; set; }
        public string enddate { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public string profile { get; set; }

        public int recpriority { get; set; }
        public int autoexpire { get; set; }
        public int maxepisodes { get; set; }
        public int maxnewest { get; set; }
        public int startoffset { get; set; }
        public int endoffset { get; set; }

        public string recgroup { get; set; }
        public int dupmethod { get; set; }
        public int dupin { get; set; }
        public string dupmethodtext { get; set; }
        public string dupintext { get; set; }
        public string station { get; set; }
        public string seriesid { get; set; }
        public string programid { get; set; }
        public int search { get; set; }

        public int autotranscode { get; set; }
        public int autocommflag { get; set; }
        public int autouserjob1 { get; set; }
        public int autouserjob2 { get; set; }
        public int autouserjob3 { get; set; }
        public int autouserjob4 { get; set; }
        public int autometalookup { get; set; }

        public int findday { get; set; }
        public string findtime { get; set; }
        public int findid { get; set; }
        public int inactive { get; set; }
        public int parentid { get; set; }
        public int transcoder { get; set; }
        public string playgroup { get; set; }
        public int prefinput { get; set; }
        public string next_record { get; set; }
        public string last_record { get; set; }
        public string last_delete { get; set; }
        public string storagegroup { get; set; }

        public string season { get; set; }
        public string episode { get; set; }
        public string inetref { get; set; }
        public int filter { get; set; }
        public string searchtype { get; set; }


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
