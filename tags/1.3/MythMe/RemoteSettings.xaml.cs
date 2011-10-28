using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace MythMe
{
    public partial class RemoteSettings : PhoneApplicationPage
    {
        public RemoteSettings()
        {
            InitializeComponent();
            
            remotePicker.ItemsSource = App.ViewModel.Frontends;

            newRemote = false;
        }

        private FrontendViewModel currentFrontend;
        private bool newRemote;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.ViewModel.Frontends.Count > 0)
            {

                remotePicker.ItemsSource = App.ViewModel.Frontends;
                remotePicker.SelectedIndex = App.ViewModel.appSettings.RemoteIndexSetting;

                if (App.ViewModel.Frontends.Count > 0)
                {
                    currentFrontend = App.ViewModel.Frontends[App.ViewModel.appSettings.RemoteIndexSetting];

                    nameBox.Text = currentFrontend.Name;
                    addressBox.Text = currentFrontend.Address;
                    portBox.Text = currentFrontend.Port.ToString();
                }

            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.ViewModel.appSettings.RemoteIndexSetting = remotePicker.SelectedIndex;
            
            base.OnNavigatedFrom(e);
        }

        private void remotePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (App.ViewModel.Frontends.Count > 0)
            {
                App.ViewModel.appSettings.RemoteIndexSetting = remotePicker.SelectedIndex;
                //remotePicker.SelectedIndex = App.ViewModel.appSettings.RemoteIndexSetting;
                remotePicker.SelectedItem = App.ViewModel.Frontends[App.ViewModel.appSettings.RemoteIndexSetting];

                currentFrontend = App.ViewModel.Frontends[App.ViewModel.appSettings.RemoteIndexSetting];

                nameBox.Text = currentFrontend.Name;
                addressBox.Text = currentFrontend.Address;
                portBox.Text = currentFrontend.Port.ToString();
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            currentFrontend = new FrontendViewModel();

            currentFrontend.Name = nameBox.Text;
            currentFrontend.Address = addressBox.Text;
            currentFrontend.Port = int.Parse(portBox.Text);

            if (newRemote)
            {
                App.ViewModel.Frontends.Add(currentFrontend);
                App.ViewModel.saveFrontends();

                remotePicker.ItemsSource = App.ViewModel.Frontends;

                App.ViewModel.appSettings.RemoteIndexSetting = App.ViewModel.Frontends.Count - 1;

                remotePicker.SelectedIndex = App.ViewModel.appSettings.RemoteIndexSetting;
                remotePicker.SelectedItem = App.ViewModel.Frontends[App.ViewModel.appSettings.RemoteIndexSetting];
            }
            else
            {
                App.ViewModel.Frontends[App.ViewModel.appSettings.RemoteIndexSetting] = currentFrontend;
                App.ViewModel.saveFrontends();

                remotePicker.ItemsSource = App.ViewModel.Frontends;

                remotePicker.SelectedIndex = App.ViewModel.appSettings.RemoteIndexSetting;
                remotePicker.SelectedItem = App.ViewModel.Frontends[App.ViewModel.appSettings.RemoteIndexSetting];
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            newRemote = true;

            currentFrontend = new FrontendViewModel();

            nameBox.Text = "";
            addressBox.Text = "";
            portBox.Text = "6546";

        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (newRemote)
            {
                currentFrontend = new FrontendViewModel();

                nameBox.Text = "";
                addressBox.Text = "";
                portBox.Text = "6546";
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to delete this frontend?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    currentFrontend = App.ViewModel.Frontends[App.ViewModel.appSettings.RemoteIndexSetting];

                    App.ViewModel.Frontends.Remove(currentFrontend);
                    App.ViewModel.saveFrontends();

                    remotePicker.ItemsSource = App.ViewModel.Frontends;
                }
            }
        }
    }
}