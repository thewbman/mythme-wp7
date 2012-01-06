using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using System.Xml.Linq;
using System.Security.Cryptography;
using Coding4Fun.Phone.Controls;

namespace MythMe
{
    public partial class Videos : PhoneApplicationPage
    {
        public Videos()
        {
            InitializeComponent();

            TotalVideos = new List<VideoViewModel>();
        }

        private List<VideoViewModel> TotalVideos;

        private List<VideoViewModel> AllVideos = new List<VideoViewModel>();
        private List<VideoViewModel> RegularVideos = new List<VideoViewModel>();
        private List<VideoViewModel> SpecialsVideos = new List<VideoViewModel>();
        private List<VideoViewModel> TvVideos = new List<VideoViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            videoPivot.SelectedIndex = App.ViewModel.appSettings.VideoIndexSetting;

            if (TotalVideos.Count == 0)
            {
                this.GetVideos();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.ViewModel.appSettings.VideoIndexSetting = videoPivot.SelectedIndex;

            base.OnNavigatedFrom(e);
        }


        private void GetVideos()
        {
            performanceProgressBarCustomized.IsIndeterminate = true;

            AllTitle.Header = "all";
            RegularTitle.Header = "regular";
            SpecialsTitle.Header = "specials";
            TvTitle.Header = "tv";

            AllVideosLL.ItemsSource = null;
            RegularVideosLL.ItemsSource = null;
            SpecialsVideosLL.ItemsSource = null;
            TvVideosLL.ItemsSource = null;

            TotalVideos.Clear();

            try
            {

                string prequery = "SET character_set_results = 'ascii';";

                string query = "SELECT videometadata.intid, videometadata.title, videometadata.subtitle, videometadata.plot, videometadata.releasedate, ";
                query += " videometadata.homepage, videometadata.director, videometadata.year, videometadata.rating, videometadata.length, ";	//asdf
                query += " videometadata.hash, videometadata.host, videometadata.insertdate, videometadata.inetref, ";	//asdf
                query += " videocategory.category, videometadata.coverfile, videometadata.season, videometadata.episode, videometadata.filename ";
                query += " FROM videometadata ";
                query += " LEFT OUTER JOIN videocategory ON videocategory.intid = videometadata.category ";


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponsePre&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&prequery64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(prequery)) + "&rand=" + App.ViewModel.randText()));
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse64&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                webRequest.BeginGetResponse(new AsyncCallback(VideosCallback), webRequest);

            }
            catch(Exception ex)
            {
                MessageBox.Show("Error trying to get videos: " + ex.ToString());
            }

        }
        private void VideosCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get videos data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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


                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<VideoViewModel>));

                TotalVideos = (List<VideoViewModel>)s.ReadObject(response.GetResponseStream());
                //TotalVideos = (List<VideoViewModel>)s.ReadObject(resultStream);
                

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got videos: " + Videos.Count);

                    for (int i = 0; i < TotalVideos.Count; i++)
                    {

                        TotalVideos[i].coverart = "http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/pl/coverart/" + TotalVideos[i].coverfile;

                        if (App.ViewModel.appSettings.VideoListImagesSetting)
                            TotalVideos[i].showCoverartList = System.Windows.Visibility.Visible;
                        else
                            TotalVideos[i].showCoverartList = System.Windows.Visibility.Collapsed;


                        if (App.ViewModel.appSettings.VideoDetailsImageSetting)
                            TotalVideos[i].showCoverartDetails = System.Windows.Visibility.Visible;
                        else
                            TotalVideos[i].showCoverartDetails = System.Windows.Visibility.Collapsed;


                        TotalVideos[i].alpha = App.ViewModel.functions.FirstChar(TotalVideos[i].title);


                        if (TotalVideos[i].season == 0)
                        {
                            if (TotalVideos[i].episode == 0)
                            {
                                TotalVideos[i].fullEpisode = "N/A";
                                TotalVideos[i].seasonText = "None";
                                TotalVideos[i].episodeText = "None";
                                TotalVideos[i].group = "Regular";
                            }
                            else if (TotalVideos[i].episode < 10)
                            {
                                TotalVideos[i].fullEpisode = "Special0" + TotalVideos[i].episode.ToString();
                                TotalVideos[i].seasonText = "Specials";
                                TotalVideos[i].episodeText = TotalVideos[i].episode.ToString();
                                TotalVideos[i].group = "Specials";
                            }
                            else
                            {
                                TotalVideos[i].fullEpisode = "Special" + TotalVideos[i].episode.ToString();
                                TotalVideos[i].seasonText = "Specials";
                                TotalVideos[i].episodeText = TotalVideos[i].episode.ToString();
                                TotalVideos[i].group = "Specails";
                            }

                        }
                        else
                        {
                            if (TotalVideos[i].season < 10)
                            {
                                TotalVideos[i].fullEpisode = "S0" + TotalVideos[i].season.ToString();
                                TotalVideos[i].seasonText = "Season  " + TotalVideos[i].season.ToString();
                            }
                            else
                            {
                                TotalVideos[i].fullEpisode = "S" + TotalVideos[i].season.ToString();
                                TotalVideos[i].seasonText = "Season " + TotalVideos[i].season.ToString();
                            }

                            if (TotalVideos[i].episode < 10)
                            {
                                TotalVideos[i].fullEpisode += "E0" + TotalVideos[i].episode.ToString();
                                TotalVideos[i].episodeText = "Episode  " + TotalVideos[i].episode.ToString();
                            }
                            else
                            {
                                TotalVideos[i].fullEpisode += "E" + TotalVideos[i].episode.ToString();
                                TotalVideos[i].episodeText = "Episode " + TotalVideos[i].episode.ToString();
                            }

                            TotalVideos[i].group = "TV";
                        }

                    }

                    //this.SortAndDisplay();
                    this.GetStoragegroups();

                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error getting videos: " + ex.ToString());
                    //this.SortAndDisplay();
                    this.GetStoragegroups();
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
                this.SortAndDisplay();
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

                    this.SortAndDisplay();

                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error getting storage groups: " + ex.ToString());
                    this.SortAndDisplay();
                });
            }

        }

        private void SortAndDisplay()
        {

            AllVideos.Clear();
            RegularVideos.Clear();
            SpecialsVideos.Clear();
            TvVideos.Clear();

            var y = TotalVideos.OrderBy(x => x.fullEpisode);

            //sorting

            foreach (var item in y)
            {

                AllVideos.Add(item);

                if (item.group == "Regular")
                {
                    RegularVideos.Add(item);
                }

                if (item.group == "Specials")
                {
                    SpecialsVideos.Add(item);
                }

                if (item.group == "TV")
                {
                    TvVideos.Add(item);
                }

            }


            var a = AllVideos.OrderBy(x => x.title).ToArray();
            var b = RegularVideos.OrderBy(x => x.title).ToArray();
            var d = SpecialsVideos.OrderBy(x => x.title).ToArray();
            var e = TvVideos.OrderBy(x => x.title).ToArray();


            var GroupedAll = from t in a
                                     //group t by t.starttime.Substring(0, 10) into c
                                     group t by t.alpha into c
                                     //orderby c.Key
                                     select new Group<VideoViewModel>(c.Key, c);
            var GroupedRegular = from t in b
                                             //group t by t.starttime.Substring(0, 10) into c
                                             group t by t.alpha into c
                                             //orderby c.Key
                                 select new Group<VideoViewModel>(c.Key, c);
            var GroupedSpecials = from t in d
                                           //group t by t.starttime.Substring(0, 10) into c
                                           group t by t.title into c
                                           //orderby c.Key
                                  select new Group<VideoViewModel>(c.Key, c);
            var GroupedTv = from t in e
                                          //group t by t.starttime.Substring(0, 10) into c
                                          group t by t.title into c
                                          //orderby c.Key
                            select new Group<VideoViewModel>(c.Key, c);


            AllVideosLL.ItemsSource = GroupedAll;
            RegularVideosLL.ItemsSource = GroupedRegular;
            SpecialsVideosLL.ItemsSource = GroupedSpecials;
            TvVideosLL.ItemsSource = GroupedTv;


            AllTitle.Header = "all (" + AllVideos.Count + ")";
            RegularTitle.Header = "regular (" + RegularVideos.Count + ")";
            SpecialsTitle.Header = "specials (" + SpecialsVideos.Count + ")";
            TvTitle.Header = "tv (" + TvVideos.Count + ")";

            performanceProgressBarCustomized.IsIndeterminate = false;

        }





        public class Group<T> : IEnumerable<T>
        {
            public Group(string name, IEnumerable<T> items)
            {
                this.Title = name;
                this.Items = new List<T>(items);
            }

            public override bool Equals(object obj)
            {
                Group<T> that = obj as Group<T>;

                return (that != null) && (this.Title.Equals(that.Title));
            }

            public string Title
            {
                get;
                set;
            }

            public IList<T> Items
            {
                get;
                set;
            }

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator()
            {
                return this.Items.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.Items.GetEnumerator();
            }

            #endregion
        }

        private void Perform(Action myMethod, int delayInMilliseconds)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (s, e) => Thread.Sleep(delayInMilliseconds);

            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();

            worker.RunWorkerAsync();
        }



        private void appbarRefresh_Click(object sender, EventArgs e)
        {
            this.GetVideos();
        }



        private void AllVideosLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (AllVideosLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedVideo = (VideoViewModel)AllVideosLL.SelectedItem;

            NavigationService.Navigate(new Uri("/VideoDetails.xaml", UriKind.Relative));

            AllVideosLL.SelectedItem = null;
        }

        private void RegularVideosLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegularVideosLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedVideo = (VideoViewModel)RegularVideosLL.SelectedItem;

            NavigationService.Navigate(new Uri("/VideoDetails.xaml", UriKind.Relative));

            RegularVideosLL.SelectedItem = null;
        }

        private void SpecialsVideosLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpecialsVideosLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedVideo = (VideoViewModel)SpecialsVideosLL.SelectedItem;

            NavigationService.Navigate(new Uri("/VideoDetails.xaml", UriKind.Relative));

            SpecialsVideosLL.SelectedItem = null;
        }

        private void TvVideosLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TvVideosLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedVideo = (VideoViewModel)TvVideosLL.SelectedItem;

            NavigationService.Navigate(new Uri("/VideoDetails.xaml", UriKind.Relative));

            TvVideosLL.SelectedItem = null;
        }
    }
}