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
            Videos = new List<VideoViewModel>();
        }

        private List<ProgramViewModel> Programs;
        private List<VideoViewModel> Videos;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string source;

            if (NavigationContext.QueryString.TryGetValue("Source", out source))
            {
                performanceProgressBarCustomized.IsIndeterminate = true;

                
                this.Perform(() => StartProgramSearch(), 50);
            }
            else
            {
                performanceProgressBarCustomized.IsIndeterminate = false;
            }

            //NavigationContext.QueryString.Clear();
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
            //VideosLL.ItemsSource = null;


            this.Programs.Clear();
            this.Videos.Clear();

            this.GetPrograms();
        }

        private void GetPrograms()
        {
            try
            {

		        string query = "SELECT `program`.title, `program`.subtitle AS subtitle, `program`.chanid AS chanid";
		        query += ", `program`.starttime AS starttime, `program`.endtime AS endtime";
		        query += ", `program`.category, `program`.originalairdate AS airdate";
		        query += ", `channel`.callsign, `channel`.channum, `channel`.name AS channame";
                query += " FROM `program` ";
		        query += " LEFT OUTER JOIN `channel` ON `program`.chanid = `channel`.chanid ";
		        query += " WHERE `title` LIKE '%"+searchBox.Text+"%' ";
		        query += " ORDER BY starttime, channum ";
		        query += " LIMIT 1000 ";


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
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
                    }

                    this.GetVideos();

                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Error getting people: " + ex.ToString());
                    this.GetVideos();
                });
            }

        }

        private void GetVideos()
        {
            string inVideoPersonId = App.ViewModel.SelectedPerson.videoPersonId;

            try
            {


                string query = "SELECT `videocast`.`cast` AS name, `videocast`.`intid` AS videoPersonId,  ";
                query += " videometadata.intid, videometadata.title, videometadata.subtitle, videometadata.plot, videometadata.inetref,  ";
                query += " videometadata.homepage, videometadata.releasedate, videometadata.season, videometadata.episode, videometadata.filename, ";
                query += " videometadata.director, videometadata.year, videometadata.rating, videometadata.length, videocategory.category, ";
                query += " videometadata.hash, videometadata.coverfile, videometadata.host, videometadata.insertdate, ";
                query += " 'video' AS type ";
                query += " FROM `videocast` ";
                query += " LEFT OUTER JOIN `videometadatacast` ON `videometadatacast`.`idcast` = `videocast`.`intid` ";
                query += " LEFT OUTER JOIN `videometadata` ON `videometadata`.`intid` = `videometadatacast`.`idvideo` ";
                query += " LEFT OUTER JOIN videocategory ON videocategory.intid = videometadata.category ";
                query += " WHERE `videocast`.`intid` = " + inVideoPersonId + " ";
                query += " LIMIT 1000 ";


                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                //webRequest.BeginGetResponse(new AsyncCallback(VideosCallback), webRequest);

                SortAndDisplay();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error requesting programs data: " + ex.ToString());
            }

        }
        private void VideosCallback(IAsyncResult asynchronousResult)
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
                //List<PeopleModel> lp = new List<PeopleModel>();

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<VideoViewModel>));

                Videos = (List<VideoViewModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got programs: " + Programs.Count);

                    for (int i = 0; i < Videos.Count; i++)
                    {

                        Videos[i].coverart = "http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/pl/coverart/" + Videos[i].coverfile;

                        if (App.ViewModel.appSettings.VideoListImagesSetting)
                            Videos[i].showCoverartList = System.Windows.Visibility.Visible;
                        else
                            Videos[i].showCoverartList = System.Windows.Visibility.Collapsed;


                        if (App.ViewModel.appSettings.VideoDetailsImageSetting)
                            Videos[i].showCoverartDetails = System.Windows.Visibility.Visible;
                        else
                            Videos[i].showCoverartDetails = System.Windows.Visibility.Collapsed;

                        if (Videos[i].season == 0)
                        {
                            if (Videos[i].episode == 0)
                            {
                                Videos[i].fullEpisode = "N/A";
                                Videos[i].seasonText = "None";
                                Videos[i].group = "Regular";
                            }
                            else if (Videos[i].episode < 10)
                            {
                                Videos[i].fullEpisode = "Special0" + Videos[i].episode.ToString();
                                Videos[i].seasonText = "Specials";
                                Videos[i].group = "Specails";
                            }
                            else
                            {
                                Videos[i].fullEpisode = "Special" + Videos[i].episode.ToString();
                                Videos[i].seasonText = "Specials";
                                Videos[i].group = "Specails";
                            }

                        }
                        else
                        {
                            if (Videos[i].season < 10)
                            {
                                Videos[i].fullEpisode = "S0" + Videos[i].season.ToString();
                                Videos[i].seasonText = "Season  " + Videos[i].season.ToString();
                            }
                            else
                            {
                                Videos[i].fullEpisode = "S" + Videos[i].season.ToString();
                                Videos[i].seasonText = "Season " + Videos[i].season.ToString();
                            }

                            if (Videos[i].episode < 10)
                            {
                                Videos[i].fullEpisode += "E0" + Videos[i].episode.ToString();
                            }
                            else
                            {
                                Videos[i].fullEpisode += "E" + Videos[i].episode.ToString();
                            }

                            Videos[i].group = "TV";
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




            var v = Videos.OrderBy(x => x.title).ToArray();

            var GroupedVideos = from t in v
                                //group t by t.starttime.Substring(0, 10) into c
                                group t by t.title into c
                                //orderby c.Key
                                select new Group<VideoViewModel>(c.Key, c);


            //VideosLL.ItemsSource = GroupedVideos;

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

        }
    }
}