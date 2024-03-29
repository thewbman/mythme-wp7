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
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Runtime.Serialization.Json;

namespace MythMe
{
    public partial class SearchProgramDetails : PhoneApplicationPage
    {
        public SearchProgramDetails()
        {

            InitializeComponent();

            People = new List<PeopleViewModel>();
            encoder = new UTF8Encoding();

            DataContext = App.ViewModel.SelectedSearchProgram;
        }

        private List<PeopleViewModel> People;
        private UTF8Encoding encoder;

        private string getDetails25String = "http://{0}:{1}/Guide/GetProgramDetails?StartTime={2}&ChanId={3}&random={2}";
        private string getDetailsString = "http://{0}:{1}/Myth/GetProgramDetails?StartTime={2}&ChanId={3}&random={4}";



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.ViewModel.appSettings.UseScriptSetting)
            {
                peoplePivot.Visibility = System.Windows.Visibility.Visible;
                setupSchedulebutton.Visibility = System.Windows.Visibility.Visible;
                titleSearchButton.Visibility = System.Windows.Visibility.Visible;

            }
            else
            {
                peoplePivot.Visibility = System.Windows.Visibility.Collapsed;
                setupSchedulebutton.Visibility = System.Windows.Visibility.Collapsed;
                titleSearchButton.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.Perform(() => GetDetails(), 50);
        }

        private void GetDetails()
        {

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                //0.25 uses UTC time
                string newStartTime = DateTime.Parse(App.ViewModel.SelectedSearchProgram.starttime).ToUniversalTime().ToString("s");

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetails25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, newStartTime, App.ViewModel.SelectedSearchProgram.chanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Details25Callback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetailsString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedSearchProgram.starttime, App.ViewModel.SelectedSearchProgram.chanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(DetailsCallback), webRequest);
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.1.105/dropbox/GetProgramDetails.xml"));
                //webRequest.BeginGetResponse(new AsyncCallback(Details25Callback), webRequest);
            }
        }


        private void Details25Callback(IAsyncResult asynchronousResult)
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

                XDocument xdoc = XDocument.Parse(resultString, LoadOptions.None);

                XElement singleProgramElement = xdoc.Element("Program");

                if (singleProgramElement.Element("Title").FirstNode != null) App.ViewModel.SelectedSearchProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                if (singleProgramElement.Element("SubTitle").FirstNode != null) App.ViewModel.SelectedSearchProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                //App.ViewModel.SelectedSearchProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                if (singleProgramElement.Element("Category").FirstNode != null) App.ViewModel.SelectedSearchProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                if (singleProgramElement.Element("FileSize").FirstNode != null) App.ViewModel.SelectedSearchProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                if (singleProgramElement.Element("SeriesId").FirstNode != null) App.ViewModel.SelectedSearchProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                if (singleProgramElement.Element("HostName").FirstNode != null) App.ViewModel.SelectedSearchProgram.hostname = (string)singleProgramElement.Element("HostName").FirstNode.ToString();
                //App.ViewModel.SelectedSearchProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                if (singleProgramElement.Element("ProgramId").FirstNode != null) App.ViewModel.SelectedSearchProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                if (singleProgramElement.Element("Season").FirstNode != null) App.ViewModel.SelectedSearchProgram.season = (string)singleProgramElement.Element("Season").FirstNode.ToString();
                if (singleProgramElement.Element("Episode").FirstNode != null) App.ViewModel.SelectedSearchProgram.episode = (string)singleProgramElement.Element("Episode").FirstNode.ToString();
                //App.ViewModel.SelectedSearchProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                //App.ViewModel.SelectedSearchProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                if (singleProgramElement.Element("EndTime").FirstNode != null)
                {
                    DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("EndTime").FirstNode.ToString());

                    App.ViewModel.SelectedSearchProgram.endtime = newEndTime.ToLocalTime().ToString("s");
                    App.ViewModel.SelectedSearchProgram.endtimespace = newEndTime.ToLocalTime().ToString("s").Replace("T", " ");
                }
                if (singleProgramElement.Element("Airdate").FirstNode != null) App.ViewModel.SelectedSearchProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                if (singleProgramElement.Element("StartTime").FirstNode != null)
                {
                    DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("StartTime").FirstNode.ToString());

                    App.ViewModel.SelectedSearchProgram.starttime = newStartTime.ToLocalTime().ToString("s");
                    App.ViewModel.SelectedSearchProgram.starttimespace = newStartTime.ToLocalTime().ToString("s").Replace("T", " ");
                }
                //App.ViewModel.SelectedSearchProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();


                App.ViewModel.SelectedSearchProgram.description = singleProgramElement.Element("Airdate").NextNode.ToString();
                if (singleProgramElement.Element("Description").FirstNode != null) App.ViewModel.SelectedSearchProgram.description = (string)singleProgramElement.Element("Description").FirstNode.ToString();
                if (App.ViewModel.SelectedSearchProgram.description.Contains("<Inet")) App.ViewModel.SelectedSearchProgram.description = "";
                if (App.ViewModel.SelectedSearchProgram.description.Contains("<Desc")) App.ViewModel.SelectedSearchProgram.description = "";


                if (App.ViewModel.SelectedSearchProgram.subtitle == "") App.ViewModel.SelectedSearchProgram.subtitle = ".";

                if (singleProgramElement.Descendants("Channel").Count() > 0)
                {

                    if (singleProgramElement.Element("Channel").Element("InputId").FirstNode != null) App.ViewModel.SelectedSearchProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Element("InputId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChannelName").FirstNode != null) App.ViewModel.SelectedSearchProgram.channame = (string)singleProgramElement.Element("Channel").Element("ChannelName").Value;
                    if (singleProgramElement.Element("Channel").Element("SourceId").FirstNode != null) App.ViewModel.SelectedSearchProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Element("SourceId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanId").FirstNode != null) App.ViewModel.SelectedSearchProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Element("ChanId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanNum").FirstNode != null) App.ViewModel.SelectedSearchProgram.channum = (string)singleProgramElement.Element("Channel").Element("ChanNum").Value;
                    if (singleProgramElement.Element("Channel").Element("CallSign").FirstNode != null) App.ViewModel.SelectedSearchProgram.callsign = (string)singleProgramElement.Element("Channel").Element("CallSign").Value;

                }


                if (singleProgramElement.Descendants("Recording").Count() > 0)
                {
                    if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) App.ViewModel.SelectedSearchProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                    if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) App.ViewModel.SelectedSearchProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                    //App.ViewModel.SelectedSearchProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedSearchProgram.recstatus);
                    if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) App.ViewModel.SelectedSearchProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                    //if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) App.ViewModel.SelectedSearchProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                    //if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) App.ViewModel.SelectedSearchProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                    if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) App.ViewModel.SelectedSearchProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                    if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null)
                    {
                        DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("Recording").Element("StartTs").FirstNode.ToString());

                        App.ViewModel.SelectedSearchProgram.recstartts = newStartTime.ToLocalTime().ToString("s");
                    }

                    if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null)
                    {
                        DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("Recording").Element("EndTs").FirstNode.ToString());

                        App.ViewModel.SelectedSearchProgram.recendts = newEndTime.ToLocalTime().ToString("s");
                    }
                }
                else
                {
                    App.ViewModel.SelectedSearchProgram.recstatus = -20;
                }

                App.ViewModel.SelectedSearchProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedSearchProgram.recstatus);


                if (App.ViewModel.SelectedSearchProgram.recstatus == -2)
                {
                    App.ViewModel.SelectedSearchProgram.recordedfourthline = "Currently recording (" + App.ViewModel.SelectedSearchProgram.channum + " - " + App.ViewModel.SelectedSearchProgram.channame + ")";
                }
                else
                {
                    App.ViewModel.SelectedSearchProgram.recordedfourthline = App.ViewModel.SelectedSearchProgram.channum + " - " + App.ViewModel.SelectedSearchProgram.channame;
                }



                if (singleProgramElement.Element("Artwork").Element("ArtworkInfos").FirstNode != null)
                {
                    foreach (var singleArtworkInfoElement in singleProgramElement.Element("Artwork").Element("ArtworkInfos").Elements("ArtworkInfo"))
                    {
                        string arturlbase = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/";
                        //string arturlend = "";
                        string arturlend = "&Height=800&Width=1024";

                        switch (singleArtworkInfoElement.Element("Type").FirstNode.ToString())
                        {
                            case "coverart":
                                App.ViewModel.SelectedSearchProgram.coverart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                break;
                            case "fanart":
                                App.ViewModel.SelectedSearchProgram.fanart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                break;
                            case "banner":
                                App.ViewModel.SelectedSearchProgram.banner = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
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




                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Done updating");

                    DataContext = App.ViewModel.SelectedSearchProgram;
                    topPanorama.DataContext = App.ViewModel.SelectedSearchProgram;

                    recstatustext.Text = App.ViewModel.SelectedSearchProgram.recstatustext;
                    title.Text = App.ViewModel.SelectedSearchProgram.title;
                    subtitle.Text = App.ViewModel.SelectedSearchProgram.subtitle;
                    category.Text = App.ViewModel.SelectedSearchProgram.category;
                    description.Text = App.ViewModel.SelectedSearchProgram.description;

                    starttime.Text = App.ViewModel.SelectedSearchProgram.starttime;
                    endtime.Text = App.ViewModel.SelectedSearchProgram.endtime;
                    seriesid.Text = App.ViewModel.SelectedSearchProgram.seriesid;
                    programid.Text = App.ViewModel.SelectedSearchProgram.programid;
                    airdate.Text = App.ViewModel.SelectedSearchProgram.airdate;

                    channum.Text = App.ViewModel.SelectedSearchProgram.channum;
                    channame.Text = App.ViewModel.SelectedSearchProgram.channame;
                    chanid.Text = App.ViewModel.SelectedSearchProgram.chanid.ToString();

                    recstatustext2.Text = App.ViewModel.SelectedSearchProgram.recstatustext;
                    recstartts.Text = App.ViewModel.SelectedSearchProgram.recstartts;
                    recendts.Text = App.ViewModel.SelectedSearchProgram.recendts;
                    //hostname.Text = App.ViewModel.SelectedSearchProgram.hostname;
                    recgroup.Text = App.ViewModel.SelectedSearchProgram.recgroup;

                });
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error parsing details: " + ex.ToString()); });
            }

            if (App.ViewModel.appSettings.UseScriptSetting) this.GetPeople();
        }

        private void DetailsCallback(IAsyncResult asynchronousResult)
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


                XDocument xdoc = XDocument.Parse(resultString, LoadOptions.None);

                foreach (XElement singleProgramElement in xdoc.Element("GetProgramDetailsResponse").Element("ProgramDetails").Descendants("Program"))
                {
                    App.ViewModel.SelectedSearchProgram.title = (string)singleProgramElement.Attribute("title").Value;
                    App.ViewModel.SelectedSearchProgram.subtitle = (string)singleProgramElement.Attribute("subTitle").Value;

                    //App.ViewModel.SelectedSearchProgram.programflags = (string)singleProgramElement.Attribute("programFlags").Value;
                    App.ViewModel.SelectedSearchProgram.category = (string)singleProgramElement.Attribute("category").Value;
                    if (singleProgramElement.Attributes("fileSize").Count() > 0) App.ViewModel.SelectedSearchProgram.filesize = Int64.Parse((string)singleProgramElement.Attribute("fileSize").Value);
                    App.ViewModel.SelectedSearchProgram.seriesid = (string)singleProgramElement.Attribute("seriesId").Value;
                    App.ViewModel.SelectedSearchProgram.hostname = (string)singleProgramElement.Attribute("hostname").Value;
                    //App.ViewModel.SelectedSearchProgram.cattype = (string)singleProgramElement.Attribute("catType").Value;
                    App.ViewModel.SelectedSearchProgram.programid = (string)singleProgramElement.Attribute("programId").Value;
                    //App.ViewModel.SelectedSearchProgram.repeat = (string)singleProgramElement.Attribute("repeat").Value;
                    //App.ViewModel.SelectedSearchProgram.stars = (string)singleProgramElement.Attribute("stars").Value;
                    App.ViewModel.SelectedSearchProgram.endtime = (string)singleProgramElement.Attribute("endTime").Value;
                    App.ViewModel.SelectedSearchProgram.endtimespace = (string)singleProgramElement.Attribute("endTime").Value.Replace("T", " ");
                    if (singleProgramElement.Attributes("airdate").Count() > 0) App.ViewModel.SelectedSearchProgram.airdate = (string)singleProgramElement.Attribute("airdate").Value;
                    App.ViewModel.SelectedSearchProgram.starttime = (string)singleProgramElement.Attribute("startTime").Value;
                    App.ViewModel.SelectedSearchProgram.starttimespace = (string)singleProgramElement.Attribute("startTime").Value.Replace("T", " ");
                    //App.ViewModel.SelectedSearchProgram.lastmodified = (string)singleProgramElement.Attribute("lastModified").Value;

                    App.ViewModel.SelectedSearchProgram.description = (string)singleProgramElement.FirstNode.ToString();

                    if (App.ViewModel.SelectedSearchProgram.description.Contains("<Channel")) App.ViewModel.SelectedSearchProgram.description = "";
                    if (App.ViewModel.SelectedSearchProgram.description.Contains("<Recording")) App.ViewModel.SelectedSearchProgram.description = "";


                    if (singleProgramElement.Descendants("Channel").Count() > 0)
                    {
                        App.ViewModel.SelectedSearchProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("inputId").Value);
                        App.ViewModel.SelectedSearchProgram.channame = (string)singleProgramElement.Element("Channel").Attribute("channelName").Value;
                        App.ViewModel.SelectedSearchProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("sourceId").Value);
                        App.ViewModel.SelectedSearchProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("chanId").Value);
                        App.ViewModel.SelectedSearchProgram.channum = (string)singleProgramElement.Element("Channel").Attribute("chanNum").Value;
                        App.ViewModel.SelectedSearchProgram.callsign = (string)singleProgramElement.Element("Channel").Attribute("callSign").Value;
                    }


                    if (singleProgramElement.Descendants("Recording").Count() > 0)
                    {
                        App.ViewModel.SelectedSearchProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recPriority").Value);
                        App.ViewModel.SelectedSearchProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recStatus").Value);
                        //App.ViewModel.SelectedSearchProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedSearchProgram.recstatus);
                        App.ViewModel.SelectedSearchProgram.recgroup = (string)singleProgramElement.Element("Recording").Attribute("recGroup").Value;
                        App.ViewModel.SelectedSearchProgram.recstartts = (string)singleProgramElement.Element("Recording").Attribute("recStartTs").Value;
                        App.ViewModel.SelectedSearchProgram.recendts = (string)singleProgramElement.Element("Recording").Attribute("recEndTs").Value;
                        App.ViewModel.SelectedSearchProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recordId").Value);

                    }
                    else
                    {
                        App.ViewModel.SelectedSearchProgram.recstatus = -20;
                    }

                    App.ViewModel.SelectedSearchProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedSearchProgram.recstatus);


                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Done updating");

                    DataContext = App.ViewModel.SelectedSearchProgram;
                    topPanorama.DataContext = App.ViewModel.SelectedSearchProgram;

                    recstatustext.Text = App.ViewModel.SelectedSearchProgram.recstatustext;
                    title.Text = App.ViewModel.SelectedSearchProgram.title;
                    subtitle.Text = App.ViewModel.SelectedSearchProgram.subtitle;
                    category.Text = App.ViewModel.SelectedSearchProgram.category;
                    description.Text = App.ViewModel.SelectedSearchProgram.description;

                    starttime.Text = App.ViewModel.SelectedSearchProgram.starttime;
                    endtime.Text = App.ViewModel.SelectedSearchProgram.endtime;
                    seriesid.Text = App.ViewModel.SelectedSearchProgram.seriesid;
                    programid.Text = App.ViewModel.SelectedSearchProgram.programid;
                    airdate.Text = App.ViewModel.SelectedSearchProgram.airdate;

                    channum.Text = App.ViewModel.SelectedSearchProgram.channum;
                    channame.Text = App.ViewModel.SelectedSearchProgram.channame;
                    chanid.Text = App.ViewModel.SelectedSearchProgram.chanid.ToString();

                    recstatustext2.Text = App.ViewModel.SelectedSearchProgram.recstatustext;
                    recstartts.Text = App.ViewModel.SelectedSearchProgram.recstartts;
                    recendts.Text = App.ViewModel.SelectedSearchProgram.recendts;
                    //hostname.Text = App.ViewModel.SelectedSearchProgram.hostname;
                    recgroup.Text = App.ViewModel.SelectedSearchProgram.recgroup;

                });
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error parsing details: " + ex.ToString()); });
            }

            if (App.ViewModel.appSettings.UseScriptSetting) this.GetPeople();

        }

        private void GetPeople()
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
                query += " WHERE (`credits`.`chanid` = " + App.ViewModel.SelectedSearchProgram.chanid;
                query += " AND `credits`.`starttime` = \"" + App.ViewModel.SelectedSearchProgram.starttime.Replace("T", " ") + "\" ) ";
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

        }


        private void Perform(Action myMethod, int delayInMilliseconds)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (s, e) => Thread.Sleep(delayInMilliseconds);

            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();

            worker.RunWorkerAsync();
        }

        private void scheduleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                DateTime dateResult;
                DateTime.TryParse(App.ViewModel.SelectedSearchProgram.starttime, out dateResult);

                //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
                TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
                //TimeSpan u = (dateResult - DateTime.Now);
                Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
                //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

                WebBrowserTask webopen = new WebBrowserTask();

                webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedSearchProgram.chanid + "/" + timestamp);
                webopen.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening browser.  Check your webserver address in the preferences.");
            }
        }

        private void playButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Remote.xaml?Command=playChannel", UriKind.Relative));
        }


        private static string randText()
        {
            Random random = new Random();

            return random.Next().ToString();
        }

        private void peopleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (peopleList.SelectedItem == null)
                return;


            var s = (PeopleViewModel)peopleList.SelectedItem;

            App.ViewModel.SelectedPerson = s;

            NavigationService.Navigate(new Uri("/People.xaml?Source=guide", UriKind.Relative));

            peopleList.SelectedItem = null;
        }

        private void guidebutton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedTime=" + App.ViewModel.SelectedSearchProgram.starttime.Replace("asdf", ""), UriKind.Relative));
        }

        private void titleSearchButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.SelectedTitle = App.ViewModel.SelectedSearchProgram.title;

            NavigationService.Navigate(new Uri("/Search.xaml?Source=search", UriKind.Relative));
        }

        private void setupSchedulebutton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.SelectedSetupProgram = App.ViewModel.SelectedSearchProgram;

            NavigationService.Navigate(new Uri("/SetupSchedule.xaml?Source=search", UriKind.Relative));
        }
    }
}