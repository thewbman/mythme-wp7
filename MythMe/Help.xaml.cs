using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace MythMe
{
    public partial class Help : PhoneApplicationPage
    {
        public Help()
        {
            InitializeComponent();

            this.Questions = new ObservableCollection<NameContentViewModel>();
            this.Support = new ObservableCollection<NameContentViewModel>();
            this.Apps = new ObservableCollection<NameContentViewModel>();
        }

        public ObservableCollection<NameContentViewModel> Questions { get; private set; }
        public ObservableCollection<NameContentViewModel> Support { get; private set; }
        public ObservableCollection<NameContentViewModel> Apps { get; private set; }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Questions.Clear();

            this.Questions.Add(new NameContentViewModel() { Name = "What is MythTV?", Content = "MythTV is a Free Open Source software digital video recorder (DVR) project distributed under the terms of the GNU GPL. This app relies fully relies on having a MythTV system already setup and running.  If you do not already have a MythTV setup this app is not for you." });
            this.Questions.Add(new NameContentViewModel() { Name = "What does this MythMe app do?", Content = "MythMe is an app for controlling a MythTV frontend and viewing MythTV's recorded and upcoming programs.  This app is released under the MIT open source license." });
            //this.Questions.Add(new NameContentViewModel() { Name = "Why do I have to know my backend's IP address?", Content = "Currently there is not a good way in Windows Phone apps to auto-find backends or other devices.  This may change in the future." });
            this.Questions.Add(new NameContentViewModel() { Name = "Can I watch recordings on my phone?", Content = "Only by first transcoding the files to a mobile-freindly format and making them accessbile through MythWeb.  The app homepage has a script and instructions on what you need to do to make the recordings accesible for downloading to your phone." });
            this.Questions.Add(new NameContentViewModel() { Name = "Does the app support the 0.25-development branch of MythTV?", Content = "Partially.  I don't have a 0.25 system to test with, but most things should work fine the 0.25 develpoment branch of MythTV.  If you find something that isn't working feel free to email me." });
            this.Questions.Add(new NameContentViewModel() { Name = "Why do I get a message about not being able to get all upcoming programs?", Content = "The Windows Phone environment doesn't do well when reading a lot of data from a socket.  It tends to work best on a solid Wi-Fi connection.  It is strongly recommended to use the optional script which improves the reliabilty of getting the upcoming schedule information and also adds a lot of additional functionality to the app." });
            this.Questions.Add(new NameContentViewModel() { Name = "What is the optional script and where can I get it?", Content = "Due to limits for apps on Windows Phone 7, many features are not availble without saving a small script to your master backend.  The script provides a lot of additional functionality including scheduling recordings, title search, people lookup and videos.  The script can be downloaded frmo the app homepage, linked to the right of these answers." });
            this.Questions.Add(new NameContentViewModel() { Name = "Why do many people's names, program and video details just show a '?' when they should be showing an accented or other non-standard character?", Content = "Honestly, I just couldn't figure out a way to make the app handle the non-standard characters without giving an error.  If you are a WP7 developer and have any ideas please let me know, otherwise you will just have to live with the '?' chracters." });
            this.Questions.Add(new NameContentViewModel() { Name = "What if I have trouble getting this app to work?", Content = "Try emailing the developer.  The contact information is available to right." });

            QuestionListBox.ItemsSource = this.Questions;


            this.Support.Clear();

            this.Support.Add(new NameContentViewModel() { Name = "email (preferred)", Content = "mythme.help@gmail.com" });
            this.Support.Add(new NameContentViewModel() { Name = "twitter", Content = "@webmyth_dev" });
            this.Support.Add(new NameContentViewModel() { Name = "app homepage", Content = "http://code.google.com/p/mythme-wp7" });
            this.Support.Add(new NameContentViewModel() { Name = "leave review", Content = "" });

            SupportListBox.ItemsSource = this.Support;


            this.Apps.Clear();

            this.Apps.Add(new NameContentViewModel() { Name = "AmpM", Content = "Streams music from an Ampache server" });
            this.Apps.Add(new NameContentViewModel() { Name = "KTorrentWP7", Content = "Controls a KTorrent program on a Linux PC" });
            //this.Apps.Add(new NameContentViewModel() { Name = "MythMe", Content = "MythTV remote control and viewer" });

            AppsListBox.ItemsSource = this.Apps;

        }

        private void email_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = "mythme.help@gmail.com";
            emailcomposer.Subject = "MythMe Help";
            emailcomposer.Show();

        }

        private void twitter_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri("http://twitter.com/webmyth_dev");
            webopen.Show();
        }

        private void homepage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri("http://code.google.com/p/mythme-wp7/");
            webopen.Show();

        }

        private void reviewTitle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //MarketplaceReviewTask marketReview = new MarketplaceReviewTask();
            //marketReview.Show();

            MarketplaceDetailTask marketDetail = new MarketplaceDetailTask();
            marketDetail.ContentIdentifier = "455f5645-0b06-429b-9cac-9097b10ae6d2";
            marketDetail.Show();
        }

        private void SupportListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SupportListBox.SelectedItem == null)
                return;

            var myItem = (NameContentViewModel)SupportListBox.SelectedItem;

            switch (myItem.Name)
            {
                case "email":
                case "email (preferred)":
                    email_Tap(sender, new System.Windows.Input.GestureEventArgs());
                    break;
                case "twitter":
                    twitter_Tap(sender, new System.Windows.Input.GestureEventArgs());
                    break;
                case "app homepage":
                    homepage_Tap(sender, new System.Windows.Input.GestureEventArgs());
                    break;
                case "leave review":
                    reviewTitle_Tap(sender, new System.Windows.Input.GestureEventArgs());
                    break;
            }
        }

        private void AppsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (AppsListBox.SelectedItem == null)
                return;

            var myItem = (NameContentViewModel)AppsListBox.SelectedItem;


            MarketplaceDetailTask marketDetail = new MarketplaceDetailTask();
            marketDetail.ContentIdentifier = "455f5645-0b06-429b-9cac-9097b10ae6d2";

            switch (myItem.Name)
            {
                case "AmpM":
                    marketDetail.ContentIdentifier = "c4c72638-8a65-4a57-89cb-6c186555b702";
                    break;
                case "KTorrentWP7":
                    marketDetail.ContentIdentifier = "5b791eb3-9e21-4c51-a086-24c665fbfe77";
                    break;
                case "MythMe":
                    marketDetail.ContentIdentifier = "455f5645-0b06-429b-9cac-9097b10ae6d2";
                    break;
            }

            marketDetail.Show();
        }
    }
}