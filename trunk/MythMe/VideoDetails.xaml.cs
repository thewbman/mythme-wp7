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
    public partial class VideoDetails : PhoneApplicationPage
    {
        public VideoDetails()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedVideo;

            People = new List<PeopleViewModel>();

            HasLoaded = false;

            peopleList.ItemsSource = People;
        }

        
        private List<PeopleViewModel> People;

        private bool HasLoaded;


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            try
            {
                if (!HasLoaded)
                {

                    this.GetPeople();

                    HasLoaded = true;
                }
                else
                {
                    this.GetPeople();
                }

                this.GetStoragegroups();

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

                    string query = "SELECT `videometadatacast`.`idcast`, `videometadatacast`.`idvideo`, ";
		            query += " `videocast`.`intid` AS videoPersonId, `videocast`.`cast` AS name, ";
		            query += " `people`.`person` ";
		            query += " FROM `videometadatacast` ";
		            query += " LEFT OUTER JOIN `videocast` ON `videometadatacast`.`idcast` = `videocast`.`intid` ";
		            query += " LEFT OUTER JOIN `people` ON `videocast`.`cast` = `people`.`name` ";
		            query += " WHERE `videometadatacast`.`idvideo` = \""+App.ViewModel.SelectedVideo.intid+"\" ";
		            query += " ORDER BY `name` ";


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
                //
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
                        if (People[i].person == "None")
                            People[i].person = "-1";
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

        }


        private void GetStoragegroups()
        {
            if (App.ViewModel.Storagegroups.Count == 0)
            {
                try
                {

                    string prequery = "SET character_set_results = 'ascii';";

                    string query = "SELECT * FROM storagegroup ;";


                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponsePre&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&prequery64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(prequery)) + "&rand=" + App.ViewModel.randText()));
                    //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse64&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                    webRequest.BeginGetResponse(new AsyncCallback(StoragegroupCallback), webRequest);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error trying to get storagegroups: " + ex.ToString());
                }
            }
            else
            {
                //
            }
        }
        private void StoragegroupCallback(IAsyncResult asynchronousResult)
        {
            //string resultString;

            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            HttpWebResponse response;

            //string resultString;
            //byte[] resultBytes;
            //Stream resultStream;

            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Failed to get storage group data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });

                return;
            }

            /*
            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                resultBytes = Convert.FromBase64String(streamReader1.ReadToEnd());
            }

            response.GetResponseStream().Close();
            response.Close();

            resultStream = new MemoryStream(resultBytes);
            */

            try
            {

                List<StoragegroupViewModel> l = new List<StoragegroupViewModel>();

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<StoragegroupViewModel>));

                l = (List<StoragegroupViewModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got storage groups: " + l.Count);

                    for (int i = 0; i < l.Count; i++)
                    {

                        App.ViewModel.Storagegroups.Add(l[i]);

                    }


                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error getting storage groups: " + ex.ToString());
                });
            }

        }


        private void peopleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (peopleList.SelectedItem == null)
                return;


            var s = (PeopleViewModel)peopleList.SelectedItem;

            App.ViewModel.SelectedPerson = s;

            peopleList.SelectedItem = null;

            NavigationService.Navigate(new Uri("/People.xaml?Source=video", UriKind.Relative));

        }

        private void homepageButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            try
            {

                WebBrowserTask webopen = new WebBrowserTask();

                webopen.Uri = new Uri(App.ViewModel.SelectedVideo.homepage);
                webopen.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening browser.  Check that the homepage for this video is valid.");
            }
        }

        private void playButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            string filebase = "/";

            foreach(var s in App.ViewModel.Storagegroups)
            {
                if((s.groupname == "Videos")&&(s.hostname == App.ViewModel.SelectedVideo.host))
                    filebase = s.dirname;
            }

            if (App.ViewModel.appSettings.ProtoVerSetting > 64)
            {
                //Fixed this playback at https://github.com/MythTV/mythtv/blob/7422f241a9c62216da5a3cfc698c3f22431cd084/mythtv/programs/mythfrontend/networkcontrol.cpp
                App.ViewModel.SelectedVideo.fullFilename = "'myth://Videos@" + App.ViewModel.SelectedVideo.host + "/" + filebase + "/" + App.ViewModel.SelectedVideo.filename + "'";
            }
            else
            {
                //Default to filename - requires local filename access to videos.  Can use NFS to fake on remote frontends.

                App.ViewModel.SelectedVideo.fullFilename = "'" + filebase + "/" + App.ViewModel.SelectedVideo.filename+"'";
            }


            NavigationService.Navigate(new Uri("/Remote.xaml?Command=playVideo", UriKind.Relative));
        }

        private void titleSearchButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.SelectedTitle = App.ViewModel.SelectedVideo.title;

            NavigationService.Navigate(new Uri("/Search.xaml?Source=video", UriKind.Relative));
        }
    }
}