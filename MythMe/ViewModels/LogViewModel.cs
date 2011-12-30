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
    public class LogViewModel : INotifyPropertyChanged
    {

        public int logid { get; set; }
        public string module { get; set; }
        public string priority { get; set; }
        public string acknolwedged { get; set; }
        public string logdate { get; set; }
        public string host { get; set; }
        public string message { get; set; }
        public string details { get; set; }


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
