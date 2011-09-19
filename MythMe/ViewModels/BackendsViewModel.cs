using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MythMe
{
    public class BackendsViewModel : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (value != _host)
                {
                    _host = value;
                    NotifyPropertyChanged("Host");
                }
            }
        }

        private int _protoPort;
        public int ProtoPort
        {
            get { return _protoPort; }
            set
            {
                if (value != _protoPort)
                {
                    _protoPort = value;
                    NotifyPropertyChanged("ProtoPort");
                }
            }
        }

        private int _xmlPort;
        public int XmlPort
        {
            get { return _xmlPort; }
            set
            {
                if (value != _xmlPort)
                {
                    _xmlPort = value;
                    NotifyPropertyChanged("XmlPort");
                }
            }
        }

        private bool _master;
        public bool Master
        {
            get { return _master; }
            set
            {
                if (value != _master)
                {
                    _master = value;
                    NotifyPropertyChanged("Master");
                }
            }
        }


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