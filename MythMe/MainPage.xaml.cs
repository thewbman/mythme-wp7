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
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();

                if ((App.ViewModel.appSettings.FirstRunSetting) || (false))
                {
                    MessageBox.Show("Welcome to MythMe.  This is an app for controlling a MythTV DVR system.  If you do not know what that means this app is not for you.  You will need to enter your master backend address in the preferences to get started.", "MythMe", MessageBoxButton.OK);

                    App.ViewModel.appSettings.FirstRunSetting = false;

                    NavigationService.Navigate(new Uri("/Preferences.xaml", UriKind.Relative));
                }

            }
            else
            {
                App.ViewModel.Connected = false;
            }


            MasterBackendTitle.Text = App.ViewModel.appSettings.MasterBackendIpSetting;
        }

        private void remoteButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //MessageBox.Show("Not yet implimented");
            NavigationService.Navigate(new Uri("/Remote.xaml", UriKind.Relative));

        }

        private void recordedButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //MessageBox.Show("Not yet implimented");
            NavigationService.Navigate(new Uri("/Recorded.xaml", UriKind.Relative));

        }

        private void upcomingButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //MessageBox.Show("Not yet implimented");
            NavigationService.Navigate(new Uri("/Upcoming.xaml", UriKind.Relative));

        }

        private void guideButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //MessageBox.Show("Not yet implimented");
            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedNow=true", UriKind.Relative));

        }

        private void searchButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void videosButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void musicButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void statusButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //MessageBox.Show("Not yet implimented");
            NavigationService.Navigate(new Uri("/Status.xaml", UriKind.Relative));

        }

        private void logButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void preferencesButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //MessageBox.Show("Not yet implimented");
            NavigationService.Navigate(new Uri("/Preferences.xaml", UriKind.Relative));

        }

        private void helpButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }
    }
}