﻿using System;
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
using System.Xml.Linq;
using System.Security.Cryptography;

namespace MythMe
{
    public partial class Status : PhoneApplicationPage
    {
        public Status()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            Encoders = new ObservableCollection<NameContentViewModel>();
            Scheduled = new ObservableCollection<NameContentViewModel>();
            Jobqueue = new ObservableCollection<NameContentViewModel>();
            Storage = new ObservableCollection<NameContentViewModel>();
            Guide = new ObservableCollection<NameContentViewModel>();
            Other = new ObservableCollection<NameContentViewModel>();

            EncodersListBox.ItemsSource = Encoders;
            ScheduledListBox.ItemsSource = Scheduled;
            JobqueueListBox.ItemsSource = Jobqueue;
            StorageListBox.ItemsSource = Storage;
            GuideListBox.ItemsSource = Guide;
            OtherListBox.ItemsSource = Other;
        }

        private string getStatus25String = "http://{0}:{1}/Status/GetStatus?random={2}";
        private string getStatusString = "http://{0}:{1}/xml?random={2}";

        ObservableCollection<NameContentViewModel> Encoders;
        ObservableCollection<NameContentViewModel> Scheduled;
        ObservableCollection<NameContentViewModel> Jobqueue;
        ObservableCollection<NameContentViewModel> Storage;
        ObservableCollection<NameContentViewModel> Guide;
        ObservableCollection<NameContentViewModel> Other;


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.ViewModel.appSettings.MasterBackendIpSetting == "")
            {
                MessageBox.Show("You need to enter a valid backend address in the preferences.");
                NavigationService.GoBack();
            }
            else
            {
                this.Perform(() => GetStatus(), 50);
            }
        }

        private void GetStatus()
        {

            performanceProgressBarCustomized.IsIndeterminate = true;

            Encoders.Clear();
            Scheduled.Clear();
            Jobqueue.Clear();
            Storage.Clear();
            Guide.Clear();
            Other.Clear();

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getStatus25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(StatusCallback), webRequest);
            }
            else
            {
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.1.105/dropbox/GetStatus.xml"));
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getStatusString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(StatusCallback), webRequest);
            }
        }


        private void StatusCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get status data: " + ex.ToString(), "Getting Error", MessageBoxButton.OK);
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

            try
            {
                //random "<!DOCTYPE Status>" at start of XML?
                XDocument xdoc = XDocument.Parse(resultString.Replace("<!DOCTYPE Status>",""), LoadOptions.None);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("Status").Attribute("version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("Status").Attribute("protoVer").Value);

                });

                
                foreach (XElement singleEncoderElement in xdoc.Element("Status").Element("Encoders").Descendants("Encoder"))
                {
                    NameContentViewModel newEncoder = new NameContentViewModel();
                    
                    string hostname;
                    string id;
                    string state;
                    string connected;

                    string title;
                    string subtitle;
                    string recstartts;
                    string recendts;
                    string channum = "";
                    string channame = "";

                    string programline = "";
                    string channelline = "";
                    string timeline = "";

                    hostname = (string)singleEncoderElement.Attribute("hostname").Value;
                    id = (string)singleEncoderElement.Attribute("id").Value;
                    state = (string)singleEncoderElement.Attribute("state").Value;
                    connected = (string)singleEncoderElement.Attribute("connected").Value;

                    foreach (XElement singleEncoderProgramElement in singleEncoderElement.Descendants("Program"))
                    {
                        title = (string)singleEncoderProgramElement.Attribute("title").Value;
                        subtitle = (string)singleEncoderProgramElement.Attribute("subTitle").Value;

                        recstartts = (string)singleEncoderProgramElement.Element("Recording").Attribute("recStartTs").Value.Replace("T"," ");
                        recendts = (string)singleEncoderProgramElement.Element("Recording").Attribute("recEndTs").Value.Replace("T", " ");

                        channum = (string)singleEncoderProgramElement.Element("Channel").Attribute("chanNum").Value;
                        channame = (string)singleEncoderProgramElement.Element("Channel").Attribute("channelName").Value;

                        programline = title + ": " + subtitle;
                        channelline = channum + " - " + channame;
                        timeline = recstartts + " to " + recendts;
                    }

                    newEncoder.Content = "#" + id + " on " + hostname;
                    newEncoder.First = App.ViewModel.functions.EncoderStateDecode(state);
                    newEncoder.Second = programline;
                    newEncoder.Third = channelline;
                    newEncoder.Fourth = timeline;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //Encoders.Add(new NameContentViewModel() { First = "#" + id + " on " + hostname, Content = "#" + id + " on " + hostname, Second = "state: " + App.ViewModel.functions.EncoderStateDecode(state), Name = "state: " + App.ViewModel.functions.EncoderStateDecode(state), Third = thirdline, Fourth = fourthline });
                        Encoders.Add(newEncoder);
                    });

                }

                foreach (XElement singleScheduledElement in xdoc.Element("Status").Element("Scheduled").Descendants("Program"))
                {
                    NameContentViewModel newScheduled = new NameContentViewModel();

                    string title;
                    string subtitle;
                    string recstartts;
                    string recendts;
                    string channum = "";
                    string channame = "";
                    string encoderid;

                    title = (string)singleScheduledElement.Attribute("title").Value;
                    subtitle = (string)singleScheduledElement.Attribute("subTitle").Value;
                    //starttime = (string)singleScheduledElement.Attribute("startTime").Value;
                    //channame = (string)singleScheduledElement.Element("Channel").Attribute("channelName").Value;
                    //channum = (string)singleScheduledElement.Element("Channel").Attribute("chanNum").Value;
                    
                    encoderid = (string)singleScheduledElement.Element("Recording").Attribute("encoderId").Value;
                    recstartts = (string)singleScheduledElement.Element("Recording").Attribute("recStartTs").Value.Replace("T", " ");
                    recendts = (string)singleScheduledElement.Element("Recording").Attribute("recEndTs").Value.Replace("T", " ");

                    channum = (string)singleScheduledElement.Element("Channel").Attribute("chanNum").Value;
                    channame = (string)singleScheduledElement.Element("Channel").Attribute("channelName").Value;

                    newScheduled.Content = title + ": " + subtitle;
                    newScheduled.First = channum + " - " + channame;
                    newScheduled.Second = recstartts + " to " + recendts;
                    newScheduled.Third = "encoder #" + encoderid;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //Scheduled.Add(new NameContentViewModel() { Content = title + ": " + subtitle, Name = starttime+" on "+channame+" (#"+encoderid+")" });
                        Scheduled.Add(newScheduled);
                    });

                }

                foreach (XElement singleJobqueueElement in xdoc.Element("Status").Element("JobQueue").Descendants("Job"))
                {
                    NameContentViewModel newJobqueue = new NameContentViewModel();
                    
                    string title;
                    string subtitle;
                    string starttime;
                    string endtime;
                    string channum = "";
                    string channame = "";
                    string type;
                    string status;
                    string comments;

                    title = (string)singleJobqueueElement.Element("Program").Attribute("title").Value;
                    subtitle = (string)singleJobqueueElement.Element("Program").Attribute("subTitle").Value;
                    starttime = (string)singleJobqueueElement.Element("Program").Attribute("startTime").Value.Replace("T", " ");
                    endtime = (string)singleJobqueueElement.Element("Program").Attribute("endTime").Value.Replace("T", " ");

                    channum = (string)singleJobqueueElement.Element("Program").Element("Channel").Attribute("chanNum").Value;
                    channame = (string)singleJobqueueElement.Element("Program").Element("Channel").Attribute("channelName").Value;

                    type = (string)singleJobqueueElement.Attribute("type").Value;
                    status = (string)singleJobqueueElement.Attribute("status").Value;
                    comments = (string)singleJobqueueElement.FirstNode.ToString();

                    if (comments.Contains("<Program")) comments = "";

                    newJobqueue.Content = title + ": " + subtitle;
                    newJobqueue.First = channum + " - " + channame;
                    newJobqueue.Second = starttime + " to " + endtime;
                    newJobqueue.Third = App.ViewModel.functions.JobqueueTypeDecode(type) + ": " + App.ViewModel.functions.JobqueueStatusDecode(status);
                    newJobqueue.Fourth = comments;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //Jobqueue.Add(new NameContentViewModel() { Content = title + ": " + subtitle, Name = App.ViewModel.functions.JobqueueTypeDecode(type)+": "+App.ViewModel.functions.JobqueueStatusDecode(status) });
                        Jobqueue.Add(newJobqueue);
                    });

                }

                foreach (XElement singleStorageElement in xdoc.Element("Status").Element("MachineInfo").Element("Storage").Descendants("Group"))
                {
                    string dir;
                    string free;
                    string total;
                    string used;

                    dir = (string)singleStorageElement.Attribute("dir").Value;
                    free = (string)singleStorageElement.Attribute("free").Value;
                    total = (string)singleStorageElement.Attribute("total").Value;
                    used = (string)singleStorageElement.Attribute("used").Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Storage.Add(new NameContentViewModel() { Content = dir, Name = "free: " + free + ", total: " + total + ", used: "+used });
                    });

                }

                foreach (XElement singleGuideElement in xdoc.Element("Status").Element("MachineInfo").Descendants("Guide"))
                {
                    string guidethru;
                    string status;
                    string next;
                    string end;
                    string guidedays;
                    string start;
                    string comments;

                    guidethru = (string)singleGuideElement.Attribute("guideThru").Value.Replace("T", " ");
                    status = (string)singleGuideElement.Attribute("status").Value;
                    next = (string)singleGuideElement.Attribute("next").Value.Replace("T", " ");
                    end = (string)singleGuideElement.Attribute("end").Value;
                    guidedays = (string)singleGuideElement.Attribute("guideDays").Value;
                    start = (string)singleGuideElement.Attribute("start").Value;
                    comments = (string)singleGuideElement.FirstNode.ToString();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Guide.Add(new NameContentViewModel() { Content = end, Name = "last run" });
                        Guide.Add(new NameContentViewModel() { Content = status, Name = "last status" });
                        Guide.Add(new NameContentViewModel() { Content = guidethru, Name = "guide data thru" });
                        Guide.Add(new NameContentViewModel() { Content = guidedays, Name = "guide days" });
                        Guide.Add(new NameContentViewModel() { Content = next, Name = "next run" });
                        //Guide.Add(new NameContentViewModel() { Content = start, Name = "last run started" });
                        Guide.Add(new NameContentViewModel() { Content = comments, Name = "comments" });
                    });

                }

                foreach (XElement singleLoadElement in xdoc.Element("Status").Element("MachineInfo").Descendants("Load"))
                {
                    string avg1;
                    string avg2;
                    string avg3;
                    string date;
                    string time;

                    avg1 = (string)singleLoadElement.Attribute("avg1").Value;
                    avg2 = (string)singleLoadElement.Attribute("avg2").Value;
                    avg3 = (string)singleLoadElement.Attribute("avg3").Value;

                    date = xdoc.Element("Status").Attribute("date").Value;
                    time = xdoc.Element("Status").Attribute("time").Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Other.Add(new NameContentViewModel() { Content = App.ViewModel.appSettings.MasterBackendIpSetting, Name = "master backend" });
                        Other.Add(new NameContentViewModel() { Content = App.ViewModel.appSettings.MythBinarySetting, Name = "mythtv version" });
                        Other.Add(new NameContentViewModel() { Content = App.ViewModel.appSettings.ProtoVerSetting.ToString(), Name = "protocol version" });
                        Other.Add(new NameContentViewModel() { Content = App.ViewModel.appSettings.DBSchemaVerSetting.ToString(), Name = "database schema version" });
                        //Other.Add(new NameContentViewModel() { Content = date, Name = "date" });
                        //Other.Add(new NameContentViewModel() { Content = time, Name = "time" });
                        Other.Add(new NameContentViewModel() { Content = date+", "+time, Name = "time" });
                        Other.Add(new NameContentViewModel() { Content = avg1 + ", " + avg2 + ", " + avg3, Name = "load average" });
                    });

                }



            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get parse data: " + ex.ToString(), "Parsing Error", MessageBoxButton.OK);
                    MessageBox.Show(resultString.ToString(), "Parsing Error", MessageBoxButton.OK);
                });

            }


            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                performanceProgressBarCustomized.IsIndeterminate = false;
            });
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
            GetStatus();
        }
    }
}