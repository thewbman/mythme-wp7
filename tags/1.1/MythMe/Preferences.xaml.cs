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

            //ChannelIcons.IsChecked = App.ViewModel.appSettings.ChannelIconsSetting;
            //images

            //remote

            if ((WebserverHost.Text == "") && (App.ViewModel.appSettings.WebserverHostSetting == "")) WebserverHost.Text = App.ViewModel.appSettings.MasterBackendIpSetting;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

            App.ViewModel.appSettings.MasterBackendIpSetting = MasterBackendIp.Text;
            App.ViewModel.appSettings.MasterBackendPortSetting = int.Parse(MasterBackendPort.Text);
            App.ViewModel.appSettings.MasterBackendXmlPortSetting = int.Parse(MasterBackendXmlPort.Text);

            //database

            //webserver
            App.ViewModel.appSettings.WebserverHostSetting = WebserverHost.Text;

            //App.ViewModel.appSettings.ChannelIconsSetting = (bool)ChannelIcons.IsChecked;
            //images

            //remote
        }

        private void appbarSave_Click(object sender, EventArgs e)
        {

            App.ViewModel.appSettings.MasterBackendIpSetting = MasterBackendIp.Text;
            App.ViewModel.appSettings.MasterBackendPortSetting = int.Parse(MasterBackendPort.Text);
            App.ViewModel.appSettings.MasterBackendXmlPortSetting = int.Parse(MasterBackendXmlPort.Text);

            //database

            //webserver
            App.ViewModel.appSettings.WebserverHostSetting = WebserverHost.Text;

            //App.ViewModel.appSettings.ChannelIconsSetting = (bool)ChannelIcons.IsChecked;
            //images

            //remote

            NavigationService.GoBack();
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((WebserverHost.Text == "") && (App.ViewModel.appSettings.WebserverHostSetting == "")) WebserverHost.Text = MasterBackendIp.Text;
        }
    }
}