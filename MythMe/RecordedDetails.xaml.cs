using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;

namespace MythMe
{
    public partial class RecordedDetails : PhoneApplicationPage
    {
        public RecordedDetails()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedProgram;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/shared/transfers"))
                {
                    isoStore.CreateDirectory("/shared/transfers");
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //text1.Text = App.ViewModel.SelectedProgram.description;

            //BitmapImage bitmapImage = new BitmapImage(new Uri(App.ViewModel.SelectedProgram.screenshot));
            //panoramaBackground.ImageSource = bitmapImage;

            if (App.ViewModel.appSettings.AllowDownloadsSetting)
            {
                downloadButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                downloadButton.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void scheduleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                DateTime dateResult;
                DateTime.TryParse(App.ViewModel.SelectedProgram.recstartts, out dateResult);

                //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
                TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
                //TimeSpan u = (dateResult - DateTime.Now);
                Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
                //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

                WebBrowserTask webopen = new WebBrowserTask();

                webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedProgram.chanid + "/" + timestamp);
                webopen.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening browser.  Check your webserver address in the preferences.");
            }
        }

        private void guideButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.GuideTime = App.ViewModel.SelectedProgram.recstartts;

            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedTime="+App.ViewModel.SelectedProgram.starttime, UriKind.Relative));
        }

        private void playButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Remote.xaml?Command=playProgram", UriKind.Relative));
        }

        private void downloadButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (BackgroundTransferService.Requests.Count() >= 5)
            {
                MessageBox.Show("The maximum number of background file transfer requests for this application has been exceeded. Please try again later.");
                return;
            }

            DateTime dateResult;
            DateTime.TryParse(App.ViewModel.SelectedProgram.recstartts, out dateResult);

            //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
            TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
            //TimeSpan u = (dateResult - DateTime.Now);
            Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
            //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;


            Uri transferUri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/pl/stream/" + App.ViewModel.SelectedProgram.chanid + "/" + timestamp + ".mp4", UriKind.RelativeOrAbsolute);

            // Create the new transfer request, passing in the URI of the file to 
            // be transferred.
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(transferUri);

            // Set the transfer method. GET and POST are supported.
            transferRequest.Method = "GET";

            string filename = App.ViewModel.SelectedProgram.starttime.Replace(":", "_") + "___" + App.ViewModel.SelectedProgram.chanid + "___" + App.ViewModel.SelectedProgram.title + "___" + App.ViewModel.SelectedProgram.subtitle + ".mp4";
            Uri downloadUri = new Uri("shared/transfers/" + filename, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;

            transferRequest.Tag = App.ViewModel.SelectedProgram.starttime.Replace(":", "_") + "___" + App.ViewModel.SelectedProgram.chanid;

            transferRequest.TransferPreferences = TransferPreferences.AllowBattery;
            
            try
            {
                BackgroundTransferService.Add(transferRequest);

                MessageBox.Show("Download has started.  You can view the video in the 'downloads' section of the app.");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Unable to add background transfer request. " + ex.Message);
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Unable to add background transfer request. " + ex2.Message);

            }

        }

    }
}