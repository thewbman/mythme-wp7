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
    public class JobqueueItem : INotifyPropertyChanged
    {

        public string id { get; set; }

        public string type { get; set; }
        //public string typeName { get; set; }

        public string status { get; set; }
        //public string statusName { get; set; }

        public string hostname { get; set; }
        public string comment { get; set; }

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
