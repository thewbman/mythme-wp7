﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace MythMe
{
    public partial class StreamPlayer : PhoneApplicationPage
    {
        public StreamPlayer()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            myMedia.Source = new Uri(App.ViewModel.SelectedStream);
            
            base.OnNavigatedTo(e);
        }
    }
}