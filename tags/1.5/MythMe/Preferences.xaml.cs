using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace MythMe
{
    public partial class Preferences : PhoneApplicationPage
    {
        public Preferences()
        {
            InitializeComponent();

            DataContext = App.ViewModel.appSettings;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            MasterBackendIp.Text = App.ViewModel.appSettings.MasterBackendIpSetting;
            MasterBackendPort.Text = App.ViewModel.appSettings.MasterBackendPortSetting.ToString();
            MasterBackendXmlPort.Text = App.ViewModel.appSettings.MasterBackendXmlPortSetting.ToString();

            //database

            //webserver
            WebserverHost.Text = App.ViewModel.appSettings.WebserverHostSetting;
            UseScript.IsChecked = App.ViewModel.appSettings.UseScriptSetting;
            PythonFileName.Text = App.ViewModel.appSettings.PythonFileSetting;
            UseServicesUpcoming.IsChecked = App.ViewModel.appSettings.UseServicesUpcomingSetting;
            AllowDownloads.IsChecked = App.ViewModel.appSettings.AllowDownloadsSetting;

            //images
            ChannelIcons.IsChecked = App.ViewModel.appSettings.ChannelIconsSetting;
            UseScriptScreenshots.IsChecked = App.ViewModel.appSettings.UseScriptScreenshotsSetting;
            VideoListImages.IsChecked = App.ViewModel.appSettings.VideoListImagesSetting;
            VideoDetailsImage.IsChecked = App.ViewModel.appSettings.VideoDetailsImageSetting;
            
            //remote

            if ((WebserverHost.Text == "") && (App.ViewModel.appSettings.WebserverHostSetting == ""))
            {
                WebserverHost.Text = App.ViewModel.appSettings.MasterBackendIpSetting;
            }


            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                UseScript.Visibility = System.Windows.Visibility.Collapsed;
                scriptcaption.Visibility = System.Windows.Visibility.Collapsed;
                PythonFileName.Visibility = System.Windows.Visibility.Collapsed;

                UseServicesUpcoming.Visibility = System.Windows.Visibility.Visible;

                UseScriptScreenshots.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                UseScript.Visibility = System.Windows.Visibility.Visible;
                scriptcaption.Visibility = System.Windows.Visibility.Visible;
                PythonFileName.Visibility = System.Windows.Visibility.Visible;

                UseServicesUpcoming.Visibility = System.Windows.Visibility.Collapsed;

                UseScriptScreenshots.Visibility = System.Windows.Visibility.Visible;
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

            App.ViewModel.appSettings.MasterBackendIpSetting = MasterBackendIp.Text;
            App.ViewModel.appSettings.MasterBackendPortSetting = int.Parse(MasterBackendPort.Text);
            App.ViewModel.appSettings.MasterBackendXmlPortSetting = int.Parse(MasterBackendXmlPort.Text);

            //database

            //webserver
            if ("" == WebserverHost.Text)
            {
                App.ViewModel.appSettings.WebserverHostSetting = "" + MasterBackendIp.Text;
            }
            else
            {
                App.ViewModel.appSettings.WebserverHostSetting = WebserverHost.Text;
            }
            App.ViewModel.appSettings.UseScriptSetting = (bool)UseScript.IsChecked;
            App.ViewModel.appSettings.PythonFileSetting = PythonFileName.Text;
            App.ViewModel.appSettings.AllowDownloadsSetting = (bool)AllowDownloads.IsChecked;
            App.ViewModel.appSettings.UseServicesUpcomingSetting = (bool)UseServicesUpcoming.IsChecked;


            //images
            App.ViewModel.appSettings.ChannelIconsSetting = (bool)ChannelIcons.IsChecked;
            App.ViewModel.appSettings.UseScriptScreenshotsSetting = (bool)UseScriptScreenshots.IsChecked;
            App.ViewModel.appSettings.VideoListImagesSetting = (bool)VideoListImages.IsChecked;
            App.ViewModel.appSettings.VideoDetailsImageSetting = (bool)VideoDetailsImage.IsChecked;

            //remote

            base.OnNavigatedFrom(e);
        }

        private void appbarSave_Click(object sender, EventArgs e)
        {


            NavigationService.GoBack();
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((WebserverHost.Text == "") && (App.ViewModel.appSettings.WebserverHostSetting == "")) WebserverHost.Text = MasterBackendIp.Text;
        }

        private void UseScript_Checked(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("You can download a script to improve the reliability of listing the upcoming recordings.  And in the future the script may also be used for additional functionality.  The script can be found on the app homepage under downloads.  (Use the help view to get the app homepage.)", "Download script", MessageBoxButton.OK);

            if (((bool)UseScript.IsChecked)&&(App.ViewModel.appSettings.DBSchemaVerSetting > 1269))
            {
                MessageBox.Show("The script is not compatible with MythTV version 0.25.");
                UseScript.IsChecked = false;
            }

        }

        private void UseScriptScreenshots_Checked(object sender, RoutedEventArgs e)
        {
            if(!(bool)UseScript.IsChecked)
            {
                MessageBox.Show("This setting can only be used if you have enabled the script for the webserver.");
                UseScriptScreenshots.IsChecked = false;
            }
        }

        private void UseScript_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if((bool)UseScript.IsChecked)
                MessageBox.Show("You can download a script to improve the reliability of listing the upcoming recordings.  And in the future the script may also be used for additional functionality.  The script can be found on the app homepage under downloads. ", "Optional script", MessageBoxButton.OK);
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri("http://code.google.com/p/mythme-wp7/");
            webopen.Show();
        }

        private void AllowDownloads_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if ((bool)AllowDownloads.IsChecked)
                MessageBox.Show("In order to play videos on your phone you need to transcode the recordings to a mobile-friendly format.  You can download the script to do that from the app homepage under downloads.", "Download", MessageBoxButton.OK);
        
        }

        private void UseServicesUpcoming_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if ((bool)UseServicesUpcoming.IsChecked)
                MessageBox.Show("If you are getting errors when loading the Upcoming program list, are connecting to a development version of MythTV or are trying to reduce bandwith usage it is recommend to enable this setting.  You will only see the programs that will actually be recorded and not all programs that match a recording rule.", "XML for upcoming", MessageBoxButton.OK);
        }
    }
}