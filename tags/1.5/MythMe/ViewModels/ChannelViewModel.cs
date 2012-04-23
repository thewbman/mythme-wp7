using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MythMe
{
    public class ChannelViewModel : INotifyPropertyChanged
    {
        public int chanid { get; set; }
        public string channum { get; set; }
        public int channumint { get; set; }
        public string callsign { get; set; }
        public string channame { get; set; }

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
