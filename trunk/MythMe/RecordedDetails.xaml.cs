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

            DataContext = App.ViewModel.SelectedRecordedProgram;
            
            Jobqueue = new List<NameContentViewModel>();
            People = new List<PeopleViewModel>();
            //encoder = new UTF8Encoding();

            HasLoaded = false;

            peopleList.ItemsSource = People;
            jobsList.ItemsSource = Jobqueue;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/shared/transfers"))
                {
                    isoStore.CreateDirectory("/shared/transfers");
                }
            }
        }


        private List<NameContentViewModel> Jobqueue;
        private List<PeopleViewModel> People;
        //private UTF8Encoding encoder;

        private bool HasLoaded;

        private int images;


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //text1.Text = App.ViewModel.SelectedRecordedProgram.description;

            //BitmapImage bitmapImage = new BitmapImage(new Uri(App.ViewModel.SelectedRecordedProgram.screenshot));
            //panoramaBackground.ImageSource = bitmapImage;


            images = 4;     //assume we have all 4
            if (App.ViewModel.SelectedRecordedProgram.fanart == null)
            {
                this.fanartButton.Visibility = System.Windows.Visibility.Collapsed;
                images--;
            }
            if (App.ViewModel.SelectedRecordedProgram.coverart == null)
            {
                this.coverartButton.Visibility = System.Windows.Visibility.Collapsed;
                images--;
            }
            if (App.ViewModel.SelectedRecordedProgram.banner == null)
            {
                this.bannerButton.Visibility = System.Windows.Visibility.Collapsed;
                images--;
            }
            if (App.ViewModel.SelectedRecordedProgram.screenshot == null)
            {
                this.screenshotButton.Visibility = System.Windows.Visibility.Collapsed;
                images--;
            }

            if (images < 1)
            {
                this.imagesPivot.Visibility = System.Windows.Visibility.Collapsed;
            }


            try
            {
                if (!HasLoaded)
                {

                    if (App.ViewModel.appSettings.AllowDownloadsSetting)
                    {
                        downloadButton.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        downloadButton.Visibility = System.Windows.Visibility.Collapsed;
                    }


                    if (App.ViewModel.SelectedRecordedProgram.fanart != null)
                    {

                        System.Windows.Media.Imaging.BitmapImage bmp = new BitmapImage(new Uri(App.ViewModel.SelectedRecordedProgram.fanart));

                        var imageBrush = new ImageBrush
                        {
                            ImageSource = bmp,
                            Opacity = 0.5d
                        };
                        
                        this.topPanorama.Background = imageBrush;

                        //int i = int.Parse("asdf");
                    }


                    if (App.ViewModel.appSettings.UseScriptSetting)
                    {
                        peoplePivot.Visibility = System.Windows.Visibility.Visible;
                        jobsPivot.Visibility = System.Windows.Visibility.Visible;
                        setupSchedulebutton.Visibility = System.Windows.Visibility.Visible;
                        titleSearchButton.Visibility = System.Windows.Visibility.Visible;

                        userjob1.Content = App.ViewModel.appSettings.UserJobDesc1Setting;
                        userjob2.Content = App.ViewModel.appSettings.UserJobDesc2Setting;
                        userjob3.Content = App.ViewModel.appSettings.UserJobDesc3Setting;
                        userjob4.Content = App.ViewModel.appSettings.UserJobDesc4Setting;

                        Jobqueue.Clear();


                        if (App.ViewModel.SelectedRecordedProgram.recgroup.ToUpper() == "DELETED")
                        {
                            deleteButton.Visibility = System.Windows.Visibility.Collapsed;
                            undeleteButton.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            deleteButton.Visibility = System.Windows.Visibility.Visible;
                            undeleteButton.Visibility = System.Windows.Visibility.Collapsed;
                        }

                        this.GetPeople();

                    }
                    else
                    {
                        peoplePivot.Visibility = System.Windows.Visibility.Collapsed;
                        jobsPivot.Visibility = System.Windows.Visibility.Collapsed;
                        setupSchedulebutton.Visibility = System.Windows.Visibility.Collapsed;
                        titleSearchButton.Visibility = System.Windows.Visibility.Collapsed;

                        deleteButton.Visibility = System.Windows.Visibility.Collapsed;
                        undeleteButton.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    HasLoaded = true;
                }
                else
                {
                    if (App.ViewModel.appSettings.UseScriptSetting)
                    {
                        this.GetPeople();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }

        private void GetPeople()
        {
            if (People.Count == 0)
            {
                try
                {

                    string prequery = "SET character_set_results = 'ascii';";

                    string query = "SELECT UPPER(`credits`.`role`) AS `role`, ";
		            query += " `people`.`name`, `people`.`person`, ";
		            query += " `videocast`.`intid` AS videoPersonId ";
		            query += " FROM `credits` ";
		            query += " LEFT OUTER JOIN `people` ON `credits`.`person` = `people`.`person` ";
		            query += " LEFT OUTER JOIN `videocast` ON `videocast`.`cast` = `people`.`name` ";
		            query += " WHERE (`credits`.`chanid` = "+App.ViewModel.SelectedRecordedProgram.chanid;
                    query += " AND `credits`.`starttime` = \"" + App.ViewModel.SelectedRecordedProgram.starttime.Replace("T", " ") + "\" ) ";
		            query += " ORDER BY `role`,`name` ";

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponsePre&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&prequery64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(prequery)) + "&rand=" + App.ViewModel.randText()));
                    //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse64&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                    webRequest.BeginGetResponse(new AsyncCallback(PeopleCallback), webRequest);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error requesting people data: " + ex.ToString());
                }
            }
            else
            {
                this.GetJobs();
            }

        }
        private void PeopleCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get people data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                //List<PeopleViewModel> lp = new List<PeopleViewModel>();

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<PeopleViewModel>));

                People = (List<PeopleViewModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got people: " + PeopleViewModel.Count);

                    for (int i = 0; i < People.Count; i++)
                    {
                        if (People[i].videoPersonId == "None")
                            People[i].videoPersonId = "-1";
                    }

                    peopleList.ItemsSource = People;

                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Error getting people: " + ex.ToString());
                });
            }

            this.GetJobs();
        }

        private void GetJobs()
        {

            try
            {
                string query = "SELECT * FROM `jobqueue` WHERE `chanid` = " + App.ViewModel.SelectedRecordedProgram.chanid + " AND `starttime` = \"" + App.ViewModel.SelectedRecordedProgram.recstartts + "\" ;";

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + randText()));
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
                List<JobqueueModel> l = new List<JobqueueModel>();

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<JobqueueModel>));

                l = (List<JobqueueModel>)s.ReadObject(response.GetResponseStream());


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

        private void mythwebButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                DateTime dateResult;
                DateTime.TryParse(App.ViewModel.SelectedRecordedProgram.recstartts, out dateResult);

                //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
                TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
                //TimeSpan u = (dateResult - DateTime.Now);
                Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
                //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

                WebBrowserTask webopen = new WebBrowserTask();

                webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedRecordedProgram.chanid + "/" + timestamp);
                webopen.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening browser.  Check your webserver address in the preferences.");
            }
        }

        private void guideButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //App.ViewModel.GuideTime = App.ViewModel.SelectedRecordedProgram.recstartts;

            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedTime="+App.ViewModel.SelectedRecordedProgram.starttime.Replace("asdf",""), UriKind.Relative));
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
            DateTime.TryParse(App.ViewModel.SelectedRecordedProgram.recstartts, out dateResult);

            //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
            TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
            //TimeSpan u = (dateResult - DateTime.Now);
            Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
            //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;


            Uri transferUri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/pl/stream/" + App.ViewModel.SelectedRecordedProgram.chanid + "/" + timestamp + ".mp4", UriKind.RelativeOrAbsolute);

            // Create the new transfer request, passing in the URI of the file to 
            // be transferred.
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(transferUri);

            // Set the transfer method. GET and POST are supported.
            transferRequest.Method = "GET";

            string filename = App.ViewModel.SelectedRecordedProgram.starttime.Replace(":", "_") + "___" + App.ViewModel.SelectedRecordedProgram.chanid + "___" + App.ViewModel.SelectedRecordedProgram.title + "___" + App.ViewModel.SelectedRecordedProgram.subtitle + ".mp4";
            Uri downloadUri = new Uri("shared/transfers/" + filename, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;

            transferRequest.Tag = App.ViewModel.SelectedRecordedProgram.starttime.Replace(":", "_") + "___" + App.ViewModel.SelectedRecordedProgram.chanid;

            //Allowing batery seems to creaet corrupted/empty downloads
            //transferRequest.TransferPreferences = TransferPreferences.AllowBattery;
            
            try
            {
                BackgroundTransferService.Add(transferRequest);

                MessageBox.Show("Download has started.  You can view the video in the 'downloads' section of the app.  Note that due to the size of the file it will only download when the phone is connected to power and a WiFi network.");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Unable to add background transfer request. " + ex.Message, "Error", MessageBoxButton.OK);
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Unable to add background transfer request. " + ex2.Message, "Error", MessageBoxButton.OK);

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
            try
            {
                string query = "INSERT INTO `jobqueue` SET `chanid` = " + App.ViewModel.SelectedRecordedProgram.chanid;
                query += ", starttime = \"" + App.ViewModel.SelectedRecordedProgram.recstartts.Replace("T", " ");
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


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + randText()));
                webRequest.BeginGetResponse(new AsyncCallback(QueueJobCallback), webRequest);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error queuing a new job: " + ex.ToString());
            }
        }
        private void QueueJobCallback(IAsyncResult asynchronousResult)
        {

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("Successfully queued job.");
            });

            this.GetJobs();
        }


        private string randText()
        {
            //Random random = new Random();

            //return random.Next().ToString();

            return App.ViewModel.randText();
        }

        private void peopleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (peopleList.SelectedItem == null)
                return;


            var s = (PeopleViewModel)peopleList.SelectedItem;

            App.ViewModel.SelectedPerson = s;

            peopleList.SelectedItem = null;

            NavigationService.Navigate(new Uri("/People.xaml?Source=recorded", UriKind.Relative));

        }

        private void titleSearchButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.SelectedTitle = App.ViewModel.SelectedRecordedProgram.title;

            NavigationService.Navigate(new Uri("/Search.xaml?Source=recorded", UriKind.Relative));
        }

        private void setupSchedulebutton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.SelectedSetupProgram = App.ViewModel.SelectedRecordedProgram;

            NavigationService.Navigate(new Uri("/SetupSchedule.xaml?Source=recorded", UriKind.Relative));

        }

        private void deleteButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string datestring = App.ViewModel.SelectedRecordedProgram.recstartts.Replace("T", "").Replace(":", "").Replace(":", "").Replace("-", "").Replace("-", "");

            string command = "DELETE_RECORDING " + App.ViewModel.SelectedRecordedProgram.chanid + " " + datestring;


            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=backendCommand&command64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(command)) + "&rand=" + randText()));
            webRequest.BeginGetResponse(new AsyncCallback(DeleteCallback), webRequest);
        }

        private void DeleteCallback(IAsyncResult asynchronousResult)
        {

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("Successfully deleted.");
            });

        }

        private void undeleteButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            try
            {
                string query = "UPDATE `recorded` SET `recgroup` = 'Default' WHERE `chanid` = ";
                query += App.ViewModel.SelectedRecordedProgram.chanid;
                query += " AND `starttime` = '";    //starttime here is actual recstartts
                query += App.ViewModel.SelectedRecordedProgram.recstartts.Replace("T"," ");
                query += "' LIMIT 1; ";


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + randText()));
                webRequest.BeginGetResponse(new AsyncCallback(UndeleteCallback), webRequest);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error queuing a new job: " + ex.ToString());
            }
        }
        private void UndeleteCallback(IAsyncResult asynchronousResult)
        {

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("Successfully undeleted.");
            });

            this.GetJobs();
        }



        private void coverartButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri(App.ViewModel.SelectedRecordedProgram.coverart);
            webopen.Show();
        }

        private void fanartButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri(App.ViewModel.SelectedRecordedProgram.fanart);
            webopen.Show();
        }

        private void bannerButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri(App.ViewModel.SelectedRecordedProgram.banner);
            webopen.Show();
        }

        private void screenshotButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri(App.ViewModel.SelectedRecordedProgram.screenshot);
            webopen.Show();
        }
    }
}