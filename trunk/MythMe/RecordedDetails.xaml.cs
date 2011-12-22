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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;

namespace MythMe
{
    public partial class RecordedDetails : PhoneApplicationPage
    {
        public RecordedDetails()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedProgram;

            Jobqueue = new ObservableCollection<NameContentViewModel>();
            encoder = new UTF8Encoding();

            jobsList.ItemsSource = Jobqueue;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/shared/transfers"))
                {
                    isoStore.CreateDirectory("/shared/transfers");
                }
            }
        }


        ObservableCollection<NameContentViewModel> Jobqueue;
        UTF8Encoding encoder;


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


            if(App.ViewModel.appSettings.UseScriptSetting)
            {
                jobsPivot.Visibility = System.Windows.Visibility.Visible;

                userjob1.Content = App.ViewModel.appSettings.UserJobDesc1Setting;
                userjob2.Content = App.ViewModel.appSettings.UserJobDesc2Setting;
                userjob3.Content = App.ViewModel.appSettings.UserJobDesc3Setting;
                userjob4.Content = App.ViewModel.appSettings.UserJobDesc4Setting;

                Jobqueue.Clear();

                this.GetJobs();

            }
            else
            {
                jobsPivot.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void GetJobs()
        {

            try
            {
                string query = "SELECT * FROM `jobqueue` WHERE `chanid` = " + App.ViewModel.SelectedProgram.chanid + " AND `starttime` = \"" + App.ViewModel.SelectedProgram.recstartts + "\" ;";

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse&query64=" + Convert.ToBase64String(encoder.GetBytes(query)) + "&rand=" + randText()));
                webRequest.BeginGetResponse(new AsyncCallback(JobsCallback), webRequest);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error requesting jobqueue data: " + ex.ToString());
            }

        }
        private void JobsCallback(IAsyncResult asynchronousResult)
        {
            //string resultString;

            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get details data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });

                return;
            }


            //using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            //{
            //    resultString = streamReader1.ReadToEnd();
            //}

            //response.GetResponseStream().Close();
            //response.Close();

            try
            {
                List<JobqueueItem> l = new List<JobqueueItem>();

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<JobqueueItem>));

                l = (List<JobqueueItem>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got jobs: " + resultString);

                    Jobqueue.Clear();

                    Jobqueue = App.ViewModel.functions.JobsToModel(l);

                    jobsList.ItemsSource = Jobqueue;

                    foreach (NameContentViewModel n in Jobqueue)
                    {
                        if (n.Name == "2")
                            comflag.Visibility = System.Windows.Visibility.Collapsed;
                        else if (n.Name == "256")
                            userjob1.Visibility = System.Windows.Visibility.Collapsed;
                        else if (n.Name == "512")
                            userjob2.Visibility = System.Windows.Visibility.Collapsed;
                        else if (n.Name == "1024")
                            userjob3.Visibility = System.Windows.Visibility.Collapsed;
                        else if (n.Name == "2048")
                            userjob4.Visibility = System.Windows.Visibility.Collapsed;

                    }
                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Error getting jobs: " + ex.ToString());
                });
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

            //Allowing batery seems to creaet corrupted/empty downloads
            //transferRequest.TransferPreferences = TransferPreferences.AllowBattery;
            
            try
            {
                BackgroundTransferService.Add(transferRequest);

                MessageBox.Show("Download has started.  You can view the video in the 'downloads' section of the app.  Note that due to the size of the file it will only download when the phone is connected to power and a WiFi network.");
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

        private void comflag_Click(object sender, RoutedEventArgs e)
        {
            callQueueJob("2");
        }

        private void userjob1_Click(object sender, RoutedEventArgs e)
        {
            callQueueJob("256");
        }

        private void userjob2_Click(object sender, RoutedEventArgs e)
        {
            callQueueJob("512");
        }

        private void userjob3_Click(object sender, RoutedEventArgs e)
        {
            callQueueJob("1024");
        }

        private void userjob4_Click(object sender, RoutedEventArgs e)
        {
            callQueueJob("2048");
        }

        private void callQueueJob(string jobTypeNum)
        {
            string query = "INSERT INTO `jobqueue` SET `chanid` = " + App.ViewModel.SelectedProgram.chanid;
            query += ", starttime = \"" + App.ViewModel.SelectedProgram.recstartts.Replace("T", " ");
            query += "\", inserttime = \"" + DateTime.Now.ToString("s").Replace("T", " ");
            query += "\", hostname = \"";
            query += "\", args = \"";
            query += "\", status = \"1";
            query += "\", statustime = \"" + DateTime.Now.ToString("s").Replace("T", " ");
            query += "\", schedruntime = \"" + DateTime.Now.ToString("s").Replace("T", " ");
            query += "\", comment = \"Queued by MythMe app";
            query += "\", flags = \"0";
            query += "\", type = " + jobTypeNum;
            query += ";";


            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(encoder.GetBytes(query)) + "&rand=" + randText()));
            webRequest.BeginGetResponse(new AsyncCallback(QueueJobCallback), webRequest);
        }

        private void QueueJobCallback(IAsyncResult asynchronousResult)
        {

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("Successfully queued job.");
            });

            this.GetJobs();
        }


        private static string randText()
        {
            Random random = new Random();

            return random.Next().ToString();
        }
    }
}