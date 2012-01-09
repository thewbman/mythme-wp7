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
using System.Runtime.Serialization.Json;

namespace MythMe
{
    public partial class Log : PhoneApplicationPage
    {
        public Log()
        {
            InitializeComponent();
        }

        List<LogViewModel> TotalLogs = new List<LogViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.GetLog();
        }

        private void GetLog()
        {
            performanceProgressBarCustomized.IsIndeterminate = true;

            TotalLogs.Clear();

            AllLL.ItemsSource = null;
            AutoexpireLL.ItemsSource = null;
            CommflagLL.ItemsSource = null;
            JobqueueLL.ItemsSource = null;
            MythbackendLL.ItemsSource = null;
            MythfilldatabaseLL.ItemsSource = null;
            SchedulerLL.ItemsSource = null;

            try
            {

                string query = "SELECT * ";
                query += " FROM `mythlog` ";
                query += " ORDER BY logdate DESC ";
                query += " LIMIT 1000 ";


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                webRequest.BeginGetResponse(new AsyncCallback(LogCallback), webRequest);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting log: " + ex.ToString());
            }
        }
        private void LogCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get log data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });

                return;
            }



            try
            {
                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<LogViewModel>));

                TotalLogs = (List<LogViewModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got log: " + TotalLogs.Count);

                    this.SortAndDisplay();

                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error getting log: " + ex.ToString());

                    this.SortAndDisplay();
                });
            }

        }

        private void SortAndDisplay()
        {

            AllLL.ItemsSource = null;
            AutoexpireLL.ItemsSource = null;
            CommflagLL.ItemsSource = null;
            JobqueueLL.ItemsSource = null;
            MythbackendLL.ItemsSource = null;
            MythfilldatabaseLL.ItemsSource = null;
            SchedulerLL.ItemsSource = null;

            List<LogViewModel> AllLogs = new List<LogViewModel>();
            List<LogViewModel> AutoexpireLogs = new List<LogViewModel>();
            List<LogViewModel> CommflagLogs = new List<LogViewModel>();
            List<LogViewModel> JobqueueLogs = new List<LogViewModel>();
            List<LogViewModel> MythbackendLogs = new List<LogViewModel>();
            List<LogViewModel> MythfilldatabaseLogs = new List<LogViewModel>();
            List<LogViewModel> SchedulerLogs = new List<LogViewModel>();

            //sorting

            foreach (var item in TotalLogs)
            {

                AllLogs.Add(item);

                if (item.module.ToLower() == "autoexpire")
                {
                    AutoexpireLogs.Add(item);
                }
                else if (item.module.ToLower() == "commflag")
                {
                    CommflagLogs.Add(item);
                }
                else if (item.module.ToLower() == "jobqueue")
                {
                    JobqueueLogs.Add(item);
                }
                else if (item.module.ToLower() == "mythbackend")
                {
                    MythbackendLogs.Add(item);
                }
                else if (item.module.ToLower() == "mythfilldatabase")
                {
                    MythfilldatabaseLogs.Add(item);
                }
                else if (item.module.ToLower() == "scheduler")
                {
                    SchedulerLogs.Add(item);
                }
            }

            var GroupedAll = from t in AllLogs
                             group t by DateTime.Parse(t.logdate).ToString("MMMM dd, yyyy") into c
                             //group t by t.module into c
                             //orderby c.Key
                             select new Group<LogViewModel>(c.Key, c);
            var GroupedAutoexpire = from t in AutoexpireLogs
                                    group t by DateTime.Parse(t.logdate).ToString("MMMM dd, yyyy") into c
                                    //group t by t.module into c
                                    //orderby c.Key
                                    select new Group<LogViewModel>(c.Key, c);
            var GroupedComflag = from t in CommflagLogs
                                 group t by DateTime.Parse(t.logdate).ToString("MMMM dd, yyyy") into c
                                 //group t by t.module into c
                                 //orderby c.Key
                                 select new Group<LogViewModel>(c.Key, c);
            var GroupedJobqueue = from t in JobqueueLogs
                                  group t by DateTime.Parse(t.logdate).ToString("MMMM dd, yyyy") into c
                                  //group t by t.module into c
                                  //orderby c.Key
                                  select new Group<LogViewModel>(c.Key, c);
            var GroupedMythbackend = from t in MythbackendLogs
                                     group t by DateTime.Parse(t.logdate).ToString("MMMM dd, yyyy") into c
                                     //group t by t.module into c
                                     //orderby c.Key
                                     select new Group<LogViewModel>(c.Key, c);
            var GroupedMythfilldatabase = from t in MythfilldatabaseLogs
                                          group t by DateTime.Parse(t.logdate).ToString("MMMM dd, yyyy") into c
                                          //group t by t.module into c
                                          //orderby c.Key
                                          select new Group<LogViewModel>(c.Key, c);
            var GroupedScheduler = from t in SchedulerLogs
                                   group t by DateTime.Parse(t.logdate).ToString("MMMM dd, yyyy") into c
                                   //group t by t.module into c
                                   //orderby c.Key
                                   select new Group<LogViewModel>(c.Key, c);


            AllLL.ItemsSource = GroupedAll;
            AutoexpireLL.ItemsSource = GroupedAutoexpire;
            CommflagLL.ItemsSource = GroupedComflag;
            JobqueueLL.ItemsSource = GroupedJobqueue;
            MythbackendLL.ItemsSource = GroupedMythbackend;
            MythfilldatabaseLL.ItemsSource = GroupedMythfilldatabase;
            SchedulerLL.ItemsSource = GroupedScheduler;


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

        private void appbarRefresh_Click(object sender, EventArgs e)
        {
            this.GetLog();
        }
    }
}