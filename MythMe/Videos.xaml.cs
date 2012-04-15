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
        private List<VideoViewModel> YearVideos = new List<VideoViewModel>();

        private List<VideoViewModel> RecentVideos = new List<VideoViewModel>();

        private string getVideos25String = "http://{0}:{1}/Video/GetVideoList?random={2}";

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
            YearTitle.Header = "year";
            RecentTitle.Header = "added";

            AllVideosLL.ItemsSource = null;
            RegularVideosLL.ItemsSource = null;
            SpecialsVideosLL.ItemsSource = null;
            TvVideosLL.ItemsSource = null;
            YearVideosLL.ItemsSource = null;

            RecentVideosLL.ItemsSource = null;

            TotalVideos.Clear();

            try
            {
                if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getVideos25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                    webRequest.BeginGetResponse(new AsyncCallback(Videos25TimeCallback), webRequest);
                }
                else
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

            }
            catch(Exception ex)
            {
                MessageBox.Show("Error trying to get videos: " + ex.ToString());
            }

        }
        private void Videos25TimeCallback(IAsyncResult asynchronousResult)
        {

            string resultString;

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
                    MessageBox.Show("Failed to get videos data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                    NavigationService.GoBack();
                });

                return;
            }

            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                resultString = streamReader1.ReadToEnd();
            }

            response.GetResponseStream().Close();
            response.Close();

            XDocument xdoc = XDocument.Parse(resultString, LoadOptions.None);


            foreach (XElement singleVideoElement in xdoc.Element("VideoMetadataInfoList").Element("VideoMetadataInfos").Elements("VideoMetadataInfo"))
            {
                VideoViewModel singleVideo = new VideoViewModel() { };

                if (singleVideoElement.Element("Id").FirstNode != null) singleVideo.intid = (string)singleVideoElement.Element("Id").Value;
                if (singleVideoElement.Element("Title").FirstNode != null) singleVideo.title = (string)singleVideoElement.Element("Title").Value;
                if (singleVideoElement.Element("SubTitle").FirstNode != null) singleVideo.subtitle = (string)singleVideoElement.Element("SubTitle").Value;
                if (singleVideoElement.Element("Tagline").FirstNode != null) singleVideo.tagline = (string)singleVideoElement.Element("Tagline").Value;
                if (singleVideoElement.Element("Director").FirstNode != null) singleVideo.director = (string)singleVideoElement.Element("Director").Value;
                if (singleVideoElement.Element("Studio").FirstNode != null) singleVideo.studio = (string)singleVideoElement.Element("Studio").Value;
                if (singleVideoElement.Element("Description").FirstNode != null) singleVideo.plot = (string)singleVideoElement.Element("Description").Value;
                if (singleVideoElement.Element("Certification").FirstNode != null) singleVideo.category = (string)singleVideoElement.Element("Certification").Value;
                if (singleVideoElement.Element("Inetref").FirstNode != null) singleVideo.inetref = (string)singleVideoElement.Element("Inetref").Value;

                if (singleVideoElement.Element("HomePage").FirstNode != null) singleVideo.homepage = (string)singleVideoElement.Element("HomePage").Value;
                if (singleVideoElement.Element("ReleaseDate").FirstNode != null) singleVideo.releasedate = singleVideoElement.Element("ReleaseDate").Value.ToString().Substring(0, 10);
                if (singleVideoElement.Element("ReleaseDate").FirstNode != null) singleVideo.year = singleVideoElement.Element("ReleaseDate").Value.ToString().Substring(0,4);
                if (singleVideoElement.Element("AddDate").FirstNode != null) singleVideo.insertdate = (string)singleVideoElement.Element("AddDate").Value;
                if (singleVideoElement.Element("Length").FirstNode != null) singleVideo.length = (string)singleVideoElement.Element("Length").Value;
                if (singleVideoElement.Element("Season").FirstNode != null) singleVideo.season = int.Parse(singleVideoElement.Element("Season").Value);
                if (singleVideoElement.Element("Episode").FirstNode != null) singleVideo.episode = int.Parse(singleVideoElement.Element("Episode").Value);
                if (singleVideoElement.Element("ContentType").FirstNode != null) singleVideo.contenttype = (string)singleVideoElement.Element("ContentType").Value;

                if (singleVideoElement.Element("FileName").FirstNode != null) singleVideo.filename = (string)singleVideoElement.Element("FileName").Value;
                if (singleVideoElement.Element("HostName").FirstNode != null) singleVideo.host = (string)singleVideoElement.Element("HostName").Value;


                if (singleVideoElement.Element("Artwork").Element("ArtworkInfos").FirstNode != null)
                {
                    foreach (var singleArtworkInfoElement in singleVideoElement.Element("Artwork").Element("ArtworkInfos").Elements("ArtworkInfo"))
                    {
                        string arturlbase = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/";
                        string arturlend = "";
                        //string arturlend = "&Height=800&Width=1024";

                        switch (singleArtworkInfoElement.Element("Type").FirstNode.ToString())
                        {
                            case "coverart":
                                singleVideo.smallcoverart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + "&Height=160";
                                singleVideo.coverart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                break;
                            case "fanart":
                                singleVideo.fanart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                break;
                            case "banner":
                                singleVideo.banner = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                break;
                            case "screenshot":
                                singleVideo.screenshot = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                break;
                            default:
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    //MessageBox.Show("Unknown Artwork: " + singleArtworkInfoElement.Element("Type").FirstNode.ToString());
                                });
                                break;
                        }
                    }
                }


                Deployment.Current.Dispatcher.BeginInvoke(() => { TotalVideos.Add(singleVideo); });
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.CreateVideoFields();

                //this.SortAndDisplay();
            });

            try
            {

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get videos data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });
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

                        if ((App.ViewModel.appSettings.VideoListImagesSetting))
                        {
                            TotalVideos[i].coverart = "http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/pl/coverart/" + TotalVideos[i].coverfile;
                            TotalVideos[i].smallcoverart = "http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/pl/coverart/" + TotalVideos[i].coverfile;
                        }


                        

                    }

                    //this.SortAndDisplay();
                    this.CreateVideoFields();

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

        private void CreateVideoFields()
        {
            for (int i = 0; i < TotalVideos.Count; i++)
            {

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
                        TotalVideos[i].group = "Specials";
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

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                this.SortAndDisplay();
            }
            else
            {
                this.GetStoragegroups();
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
            YearVideos.Clear();

            RecentVideos.Clear();

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

                YearVideos.Add(item);

                RecentVideos.Add(item);

            }


            var a = AllVideos.OrderBy(x => x.title).ToArray();
            var b = RegularVideos.OrderBy(x => x.title).ToArray();
            var d = SpecialsVideos.OrderBy(x => x.title).ToArray();
            var e = TvVideos.OrderBy(x => x.title).ToArray();
            var f = YearVideos.OrderBy(x => x.year).ToArray();

            var g = RecentVideos.OrderByDescending(x => x.insertdate).ToArray();


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
            var GroupedYear = from t in f
                            //group t by t.starttime.Substring(0, 10) into c
                            group t by t.year into c
                            //orderby c.Key
                            select new Group<VideoViewModel>(c.Key, c);

            var GroupedRecent = from t in g
                            //group t by t.starttime.Substring(0, 10) into c
                            group t by t.insertdate.Substring(0,10) into c
                            //orderby c.Key
                            select new Group<VideoViewModel>(c.Key, c);


            AllVideosLL.ItemsSource = GroupedAll;
            RegularVideosLL.ItemsSource = GroupedRegular;
            SpecialsVideosLL.ItemsSource = GroupedSpecials;
            TvVideosLL.ItemsSource = GroupedTv;
            YearVideosLL.ItemsSource = GroupedYear;

            RecentVideosLL.ItemsSource = GroupedRecent;


            AllTitle.Header = "all (" + AllVideos.Count + ")";
            RegularTitle.Header = "regular (" + RegularVideos.Count + ")";
            SpecialsTitle.Header = "specials (" + SpecialsVideos.Count + ")";
            TvTitle.Header = "tv (" + TvVideos.Count + ")";
            YearTitle.Header = "year";

            RecentTitle.Header = "added";

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

        private void RecentVideosLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecentVideosLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedVideo = (VideoViewModel)RecentVideosLL.SelectedItem;

            NavigationService.Navigate(new Uri("/VideoDetails.xaml", UriKind.Relative));

            RecentVideosLL.SelectedItem = null;
        }

        private void YearVideosLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YearVideosLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedVideo = (VideoViewModel)YearVideosLL.SelectedItem;

            NavigationService.Navigate(new Uri("/VideoDetails.xaml", UriKind.Relative));

            YearVideosLL.SelectedItem = null;
        }
    }
}