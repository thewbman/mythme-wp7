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
        }

        public ObservableCollection<NameContentViewModel> Questions { get; private set; }
        public ObservableCollection<NameContentViewModel> Support { get; private set; }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Questions.Clear();

            this.Questions.Add(new NameContentViewModel() { Name = "What is MythTV?", Content = "MythTV is a Free Open Source software digital video recorder (DVR) project distributed under the terms of the GNU GPL. This app relies fully relies on having a MythTV system already setup and running.  If you do not already have a MythTV setup this app is not for you." });
            this.Questions.Add(new NameContentViewModel() { Name = "What does this MythMe app do?", Content = "MythMe is an app for controlling a MythTV frontend and viewing MythTV's recorded and upcoming programs.  This app is released under the MIT open source license." });
            this.Questions.Add(new NameContentViewModel() { Name = "Why do I have to know my backend's IP address?", Content = "Currently there is not a good way in Windows Phone apps to auto-find backends or other devices.  This may change in the future." });
            this.Questions.Add(new NameContentViewModel() { Name = "Does the app support the 0.25-development branch of MythTV?", Content = "Not yet.  I do plan to support that in the near future." });
            this.Questions.Add(new NameContentViewModel() { Name = "Why do I get a message about not being able to get all upcoming programs?", Content = "The Windows Phone environment doesn't do well when reading a lot of data from a socket.  It tends to work best on a solid Wi-Fi connection.  In the future I may support a workaround for this issue by using a server-side script." });
            this.Questions.Add(new NameContentViewModel() { Name = "What I have trouble getting this app to work?", Content = "Try emailing the developer.  The contact information is available to right." });

            QuestionListBox.ItemsSource = this.Questions;

            this.Support.Clear();

            this.Support.Add(new NameContentViewModel() { Name = "email", Content = "mythme.help@gmail.com" });
            this.Support.Add(new NameContentViewModel() { Name = "twitter", Content = "@webmyth_dev" });
            this.Support.Add(new NameContentViewModel() { Name = "app homepage", Content = "http://code.google.com/p/mythme-wp7" });
            this.Support.Add(new NameContentViewModel() { Name = "leave review", Content = "" });

            SupportListBox.ItemsSource = this.Support;

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
    }
}