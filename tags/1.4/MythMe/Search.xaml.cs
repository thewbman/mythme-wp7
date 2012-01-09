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
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Runtime.Serialization.Json;

namespace MythMe
{
    public partial class Search : PhoneApplicationPage
    {
        public Search()
        {
            InitializeComponent();

            Programs = new List<ProgramViewModel>();
            TotalVideos = new List<VideoViewModel>();
        }

        private List<ProgramViewModel> Programs;
        private List<VideoViewModel> TotalVideos;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string source;

            if (NavigationContext.QueryString.TryGetValue("Source", out source))
            {
                performanceProgressBarCustomized.IsIndeterminate = true;

                searchBox.Text = App.ViewModel.SelectedTitle;
                
                this.Perform(() => StartProgramSearch(), 50);
            }
            else
            {
                performanceProgressBarCustomized.IsIndeterminate = false;
            }

            NavigationContext.QueryString.Clear();
        }


        private void Perform(Action myMethod, int delayInMilliseconds)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (s, e) => Thread.Sleep(delayInMilliseconds);

            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();

            worker.RunWorkerAsync();
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


        private void StartProgramSearch()
        {
            //hide keyboard
            Focus();

            performanceProgressBarCustomized.IsIndeterminate = true;

            ProgramsLL.ItemsSource = null;
            VideosLL.ItemsSource = null;


            this.Programs.Clear();
            this.TotalVideos.Clear();

            this.GetPrograms();
        }

        private void GetPrograms()
        {
            try
            {

                string prequery = "SET character_set_results = 'ascii';";

                string query = "SELECT `program`.title, `program`.subtitle AS subtitle, `program`.chanid AS chanid";
		        query += ", `program`.starttime AS starttime, `program`.endtime AS endtime";
		        query += ", `program`.category, `program`.originalairdate AS airdate";
		        query += ", `channel`.callsign, `channel`.channum, `channel`.name AS channame";
                query += " FROM `program` ";
		        query += " LEFT OUTER JOIN `channel` ON `program`.chanid = `channel`.chanid ";
		        query += " WHERE `title` LIKE '%"+searchBox.Text+"%' ";
		        query += " ORDER BY starttime, channum ";
		        query += " LIMIT 1000 ";


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponsePre&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&prequery64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(prequery)) + "&rand=" + App.ViewModel.randText()));
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse64&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                webRequest.BeginGetResponse(new AsyncCallback(ProgramsCallback), webRequest);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error requesting programs data: " + ex.ToString());
            }
        }
        private void ProgramsCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get programs data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                //List<PeopleModel> lp = new List<PeopleModel>();

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<ProgramViewModel>));

                Programs = (List<ProgramViewModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got programs: " + Programs.Count);

                    for (int i = 0; i < Programs.Count; i++)
                    {
                        Programs[i].chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + Programs[i].chanid;

                        if (App.ViewModel.appSettings.ChannelIconsSetting)
                            Programs[i].showChanicon = System.Windows.Visibility.Visible;
                        else
                            Programs[i].showChanicon = System.Windows.Visibility.Collapsed;

                        if ((Programs[i].subtitle == null) || (Programs[i].subtitle == ""))
                            Programs[i].subtitle = ".";
                    }

                    this.GetTotalVideos();

                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Error getting people: " + ex.ToString());
                    this.GetTotalVideos();
                });
            }

        }

        private void GetTotalVideos()
        {
            
            try
            {

                string prequery = "SET character_set_results = 'ascii';";

                string query = "SELECT videometadata.intid, videometadata.title, videometadata.subtitle, videometadata.plot, videometadata.releasedate, ";
                query += " videometadata.homepage, videometadata.director, videometadata.year, videometadata.rating, videometadata.length, ";	//asdf
                query += " videometadata.hash, videometadata.host, videometadata.insertdate, videometadata.inetref, ";	//asdf
                query += " videocategory.category, videometadata.coverfile, videometadata.season, videometadata.episode, videometadata.filename ";
                query += " FROM videometadata ";
                query += " LEFT OUTER JOIN videocategory ON videocategory.intid = videometadata.category ";
                query += " WHERE `title` LIKE '%" + searchBox.Text + "%' ";
                query += " ORDER BY title, season, episode ";
                query += " LIMIT 1000 ";


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponsePre&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&prequery64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(prequery)) + "&rand=" + App.ViewModel.randText()));
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse64&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                webRequest.BeginGetResponse(new AsyncCallback(TotalVideosCallback), webRequest);

                //SortAndDisplay();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error requesting TotalVideos data: " + ex.ToString());
            }

        }
        private void TotalVideosCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get videos data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                //List<PeopleModel> lp = new List<PeopleModel>();

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<VideoViewModel>));

                TotalVideos = (List<VideoViewModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got programs: " + Programs.Count);

                    for (int i = 0; i < TotalVideos.Count; i++)
                    {

                        if ((App.ViewModel.appSettings.VideoListImagesSetting))
                            TotalVideos[i].coverart = "http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/pl/coverart/" + TotalVideos[i].coverfile;

                        if (App.ViewModel.appSettings.VideoListImagesSetting)
                            TotalVideos[i].showCoverartList = System.Windows.Visibility.Visible;
                        else
                            TotalVideos[i].showCoverartList = System.Windows.Visibility.Collapsed;


                        if (App.ViewModel.appSettings.VideoDetailsImageSetting)
                            TotalVideos[i].showCoverartDetails = System.Windows.Visibility.Visible;
                        else
                            TotalVideos[i].showCoverartDetails = System.Windows.Visibility.Collapsed;

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
                                TotalVideos[i].group = "Specails";
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

                    this.SortAndDisplay();

                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Error getting people: " + ex.ToString());

                    this.SortAndDisplay();
                });
            }

        }

        private void SortAndDisplay()
        {

            switch (App.ViewModel.appSettings.SearchSortSetting)
            {
                case "channel":
                    foreach (var item in Programs)
                    {
                        item.guidesort = item.channum;
                        item.guidesortdisplay = item.channum + " - " + item.channame;
                    }
                    break;
                case "starttime":
                    foreach (var item in Programs)
                    {
                        item.guidesort = item.starttime;
                        item.guidesortdisplay = DateTime.Parse(item.starttime).ToString("dddd, MMMM dd, yyyy");
                    }
                    break;
                case "title":
                    foreach (var item in Programs)
                    {
                        item.guidesort = item.title;
                        item.guidesortdisplay = item.title;
                    }
                    break;
                default:
                    foreach (var item in Programs)
                    {
                        item.guidesort = item.title;
                        item.guidesortdisplay = item.title;
                    }
                    break;
            }


            var p = Programs.OrderBy(x => x.guidesort).ToArray();

            switch (App.ViewModel.appSettings.SearchSortAscSetting)
            {
                case true:
                    //p = Programs.OrderBy(x => x.guidesort).ToArray();
                    break;
                case false:
                    p = Programs.OrderByDescending(x => x.guidesort).ToArray();
                    break;
            }




            var GroupedPrograms = from t in p
                                  //group t by t.starttime.Substring(0, 10) into c
                                  group t by t.guidesortdisplay into c
                                  //orderby c.Key
                                  select new Group<ProgramViewModel>(c.Key, c);


            ProgramsLL.ItemsSource = GroupedPrograms;




            var v = TotalVideos.OrderBy(x => x.title).ToArray();

            var GroupedTotalVideos = from t in v
                                //group t by t.starttime.Substring(0, 10) into c
                                group t by t.title into c
                                //orderby c.Key
                                select new Group<VideoViewModel>(c.Key, c);


            VideosLL.ItemsSource = GroupedTotalVideos;

            performanceProgressBarCustomized.IsIndeterminate = false;

        }





        private void appbarSearch_Click(object sender, EventArgs e)
        {
            SearchTask s = new SearchTask();
            s.SearchQuery = searchBox.Text;

            s.Show();
        }

        private void searchBox_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key.ToString() == "Enter")
                this.StartProgramSearch();
        }

        private void searchBoxButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.StartProgramSearch();
        }

        private void ProgramsLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ProgramsLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedSearchProgram = (ProgramViewModel)ProgramsLL.SelectedItem;

            NavigationService.Navigate(new Uri("/SearchProgramDetails.xaml", UriKind.Relative));

            ProgramsLL.SelectedItem = null;
        }

        private void VideosLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //seperate video details page?

            if (VideosLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedVideo = (VideoViewModel)VideosLL.SelectedItem;

            NavigationService.Navigate(new Uri("/VideoDetails.xaml", UriKind.Relative));

            VideosLL.SelectedItem = null;

        }
    }
}