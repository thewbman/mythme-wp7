using System;
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

                if (App.ViewModel.appSettings.FirstRunSetting) MessageBox.Show("Welcome to MythMe.", "MythMe", MessageBoxButton.OK);

                App.ViewModel.appSettings.FirstRunSetting = false;

            }
            else
            {
                App.ViewModel.Connected = false;
            }


            MasterBackendTitle.Text = App.ViewModel.appSettings.MasterBackendIpSetting;
        }

        private void remoteButton_Tap(object sender, GestureEventArgs e)
        {
            //MessageBox.Show("Not yet implimented");
            NavigationService.Navigate(new Uri("/Remote.xaml", UriKind.Relative));

        }

        private void recordedButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void upcomingButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void guideButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void searchButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void videosButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void musicButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void statusButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void logButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void preferencesButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }

        private void helpButton_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");

        }
    }
}