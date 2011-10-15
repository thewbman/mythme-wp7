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
    public class NameContentViewModel : INotifyPropertyChanged
    {
        private string _Name;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _Content;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Content
        {
            get
            {
                return _Content;
            }
            set
            {
                if (value != _Content)
                {
                    _Content = value;
                    NotifyPropertyChanged("Content");
                }
            }
        }

        private string _First;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string First
        {
            get
            {
                return _First;
            }
            set
            {
                if (value != _First)
                {
                    _First = value;
                    NotifyPropertyChanged("First");
                }
            }
        }

        private string _Second;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Second
        {
            get
            {
                return _Second;
            }
            set
            {
                if (value != _Second)
                {
                    _Second = value;
                    NotifyPropertyChanged("Second");
                }
            }
        }

        private string _Third;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Third
        {
            get
            {
                return _Third;
            }
            set
            {
                if (value != _Third)
                {
                    _Third = value;
                    NotifyPropertyChanged("Third");
                }
            }
        }

        private string _Fourth;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Fourth
        {
            get
            {
                return _Fourth;
            }
            set
            {
                if (value != _Fourth)
                {
                    _Fourth = value;
                    NotifyPropertyChanged("Fourth");
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