using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.ComponentModel;
using System.Threading;
using System.Xml.Linq;
using System.IO;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;

namespace MythMe
{
    public partial class Stream : PhoneApplicationPage
    {
        public Stream()
        {
            InitializeComponent();
        }

        private string getStreamListString = "http://{0}:{1}/Content/GetLiveStreamList?random={2}";

        List<NameContentViewModel> AllStreams = new List<NameContentViewModel>();


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Perform(() => GetStreamList(), 50);
        }

        private void GetStreamList()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getStreamListString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
            webRequest.BeginGetResponse(new AsyncCallback(StreamListCallback), webRequest);
        }
        private void StreamListCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get details data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });

                return;
            }

            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                resultString = streamReader1.ReadToEnd();
            }

            response.GetResponseStream().Close();
            response.Close();


            try
            {

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    AllStreams = new List<NameContentViewModel>();
                    StreamListLL.ItemsSource = null;
                });

                XDocument xdoc = XDocument.Parse(resultString, LoadOptions.None);

                if (xdoc.Element("LiveStreamInfoList").Element("LiveStreamInfos").FirstNode != null)
                {
                    foreach (XElement singleStreamElement in xdoc.Element("LiveStreamInfoList").Element("LiveStreamInfos").Elements("LiveStreamInfo"))
                    {
                        NameContentViewModel singleStream = new NameContentViewModel() {  };

                        singleStream.First = singleStreamElement.Element("Id").FirstNode.ToString();

                        singleStream.Name = singleStreamElement.Element("SourceFile").FirstNode.ToString();
                        singleStream.Content = singleStreamElement.Element("FullURL").FirstNode.ToString();

                        singleStream.Second = singleStreamElement.Element("StatusStr").FirstNode.ToString();
                        singleStream.Third = singleStreamElement.Element("SourceHost").FirstNode.ToString();
                        singleStream.Fourth = singleStreamElement.Element("PercentComplete").FirstNode.ToString();

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            AllStreams.Add(singleStream);
                        });
                    }
                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.SortAndDisplay();
                });

            }
            catch (Exception ex)
            {

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to parse stream list data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });
            }
        }

        private void SortAndDisplay()
        {
            if (AllStreams.Count < 1)
            {
                MessageBox.Show("No streams yet");
            }
            else
            {
                //streamList.ItemsSource = AllStreams;

                var arr = AllStreams.OrderBy(x => x.Second).ToArray();

                var GroupedStreamList = from t in arr
                                             //group t by t.starttime.Substring(0, 10) into c
                                             group t by t.Second into c
                                             //orderby c.Key
                                             select new Group<NameContentViewModel>(c.Key, c);

                StreamListLL.ItemsSource = GroupedStreamList;

            }
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



        private void appbarRefresh_Click(object sender, EventArgs e)
        {
            this.GetStreamList();
        }

        private void streamList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void deleteStream_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StreamListLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (StreamListLL.SelectedItem == null)
                return;

            var s = (NameContentViewModel)StreamListLL.SelectedItem;


            App.ViewModel.SelectedStream = s.Content;

            MessageBox.Show("Opening: " + App.ViewModel.SelectedStream);

            /*
             
            MediaPlayerLauncher m = new MediaPlayerLauncher();
            m.Media = new Uri(s.Content + "&End=.wmv", UriKind.Absolute);
            m.Orientation = MediaPlayerOrientation.Landscape;
            m.Location = MediaLocationType.Data;
            m.Show();
            
             * */
            StreamListLL.SelectedItem = null;

            

            App.ViewModel.SelectedStream = s.Content;

            NavigationService.Navigate(new Uri("/StreamPlayer.xaml", UriKind.Relative));

        }
    }
}