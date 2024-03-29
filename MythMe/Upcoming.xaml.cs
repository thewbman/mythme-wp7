﻿using System;
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
    public partial class Upcoming : PhoneApplicationPage
    {
        public Upcoming()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            //AllUpcomingListBox.ItemsSource = App.ViewModel.Upcoming;

            //AllUpcomingListBox.ItemsSource = AllUpcoming;
            //ConflictingUpcomingListBox.ItemsSource = ConflictingUpcoming;
            //OverridesUpcomingListBox.ItemsSource = OverridesUpcoming;
            //UpcomingUpcomingListBox.ItemsSource = UpcomingUpcoming;
        }

        const int MAX_BUFFER_SIZE = 1460;   //this is the payload size sent from my backend 0.24.1+fixes

        private int protocolStatus;
        private int protocolResponseLength;
        private string fullProtocolResponse;

        private string getUpcoming25String = "http://{0}:{1}/Dvr/GetUpcomingList?random={2}";
        private string getConflict25String = "http://{0}:{1}/Dvr/GetConflictList?random={2}";

        ObservableCollection<ProgramViewModel> AllUpcoming = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> ConflictingUpcoming = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> OverridesUpcoming = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> UpcomingUpcoming = new ObservableCollection<ProgramViewModel>();

        private bool HasLoaded = false;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if ("" == App.ViewModel.appSettings.WebserverHostSetting)
            {
                App.ViewModel.appSettings.WebserverHostSetting = "" + App.ViewModel.appSettings.MasterBackendIpSetting;
            }

            if ((App.ViewModel.appSettings.UseScriptScreenshotsSetting) && (!App.ViewModel.appSettings.UseScriptSetting))
            {
                App.ViewModel.appSettings.UseScriptScreenshotsSetting = false;
            }

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                //App.ViewModel.appSettings.UseScriptSetting = false;
            }
            else
            {
                App.ViewModel.appSettings.UseServicesUpcomingSetting = false;
            }


            //performanceProgressBarCustomized.IsIndeterminate = true;

            if (App.ViewModel.appSettings.ProtoVerSetting == 0)
            {
                MessageBox.Show("You need to view your recorded programs first to get the protocol version before you can view the upcoming recordings.", "MythMe", MessageBoxButton.OK);
                NavigationService.GoBack();
            }
            else
            {

                if (App.ViewModel.Upcoming.Count == 0)
                {
                    GetUpcoming();
                }
                else if (!HasLoaded)
                {

                    SortAndDisplay("");

                }
                else
                {
                    //do nothing
                }
            }

        }
        
        private void GetUpcoming()
        {

            performanceProgressBarCustomized.IsIndeterminate = true;

            AllTitle.Header = "all";
            ConflictingTitle.Header = "conflicting";
            OverridesTitle.Header = "overrides";
            UpcomingTitle.Header = "upcoming";

            AllUpcoming.Clear();
            ConflictingUpcoming.Clear();
            OverridesUpcoming.Clear();
            UpcomingUpcoming.Clear();

            AllUpcomingLL.ItemsSource = null;
            ConflictingUpcomingLL.ItemsSource = null;
            OverridesUpcomingLL.ItemsSource = null;
            UpcomingUpcomingLL.ItemsSource = null;

            App.ViewModel.Upcoming.Clear();

            if (App.ViewModel.appSettings.UseServicesUpcomingSetting)
            {
                GetUpcomingServices();
            }
            else if ((App.ViewModel.appSettings.UseScriptSetting)&&(App.ViewModel.appSettings.DBSchemaVerSetting <= 1269))
            {
                //dont use script for mythtv 0.25
                GetUpcomingScript();
            }
            else 
            {
                GetUpcomingProtocol();
            }
        }

        private void GetUpcomingScript()
        {
            string url = "http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=getPending&rand=" + App.ViewModel.randText();

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            webRequest.BeginGetResponse(new AsyncCallback(UpcomingCallback), webRequest);
        }
        private void UpcomingCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get upcoming data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                    App.ViewModel.Connected = false;
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
                List<ScriptProgram> programs = new List<ScriptProgram>();

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(resultString));
                DataContractJsonSerializer x = new DataContractJsonSerializer(programs.GetType());

                programs = x.ReadObject(ms) as List<ScriptProgram>;

                ms.Close();


                foreach (ScriptProgram s in programs)
                {
                    ProgramViewModel singleProgram = new ProgramViewModel();

                    singleProgram.airdate = s.airdate;
                    singleProgram.callsign = s.callsign;
                    singleProgram.cardid = int.Parse(s.cardid);
                    singleProgram.category = s.category;
                    singleProgram.chanid = int.Parse(s.chanid);
                    singleProgram.channame = s.channame;
                    singleProgram.channum = s.channum;
                    //singleProgram.channumint = 0;
                    singleProgram.description = s.description;
                    singleProgram.endtime = s.endtime;
                    singleProgram.endtimespace = s.endtime.Replace("T", " ");
                    //singleProgram.filename = s.filename;
                    //singleProgram.filesize = Int64.Parse(s.filesize);
                    //singleProgram.findid = s.findid;
                    singleProgram.hostname = s.hostname;
                    singleProgram.inputid = int.Parse(s.inputid);
                    //singleProgram.lastmodified = s.lastmodified;
                    singleProgram.parentid = s.parentid;
                    //singleProgram.playgroup = s.playgroup;
                    singleProgram.programid = s.programid;
                    singleProgram.recendts = s.recendts;
                    singleProgram.recgroup = s.recgroup;
                    singleProgram.recordid = int.Parse(s.recordid);
                    //singleProgram.recpriority = int.Parse(s.recpriority);
                    //singleProgram.recpriority2 = s.recpriority2;
                    singleProgram.recstartts = s.recstartts;
                    singleProgram.recstatus = int.Parse(s.recstatus);
                    singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                    singleProgram.rectype = int.Parse(s.rectype);
                    singleProgram.seriesid = s.seriesid;
                    singleProgram.sourceid = int.Parse(s.sourceid);
                    singleProgram.starttime = s.starttime;
                    singleProgram.starttimespace = s.starttime.Replace("T"," ");
                    singleProgram.storagegroup = s.storagegroup;
                    singleProgram.subtitle = s.subtitle;
                    singleProgram.title = s.title;



                    //singleProgram.upcomingfourthline;

                    singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + singleProgram.chanid;
                            

                        
                    if (App.ViewModel.appSettings.ChannelIconsSetting)
                        singleProgram.showChanicon = System.Windows.Visibility.Visible;
                    else
                        singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.ViewModel.Upcoming.Add(singleProgram);
                    });
                    
            
                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SortAndDisplay("");
                    });

            
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error parsing JSON from script: " + ex.ToString());
                });
            }
             

        }

        private void GetUpcomingServices()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getUpcoming25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
            webRequest.BeginGetResponse(new AsyncCallback(Upcoming25Callback), webRequest);
        }
        private void Upcoming25Callback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get upcoming data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                    App.ViewModel.Connected = false;
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

                XDocument xdoc = XDocument.Parse(resultString, LoadOptions.None);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    App.ViewModel.Upcoming.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("ProgramList").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("ProgramList").Element("ProtoVer").Value);

                });

                //ObservableCollection<ProgramViewModel> programlist = new ObservableCollection<ProgramViewModel>();
                //int recordedCount = 0;


                foreach (XElement singleProgramElement in xdoc.Element("ProgramList").Element("Programs").Descendants("Program"))
                {
                    ProgramViewModel singleProgram = new ProgramViewModel() { };

                    if (singleProgramElement.Element("Title").FirstNode != null) singleProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                    if (singleProgramElement.Element("SubTitle").FirstNode != null) singleProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                    //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                    if (singleProgramElement.Element("Category").FirstNode != null) singleProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                    if (singleProgramElement.Element("FileSize").FirstNode != null) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                    if (singleProgramElement.Element("SeriesId").FirstNode != null) singleProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                    if (singleProgramElement.Element("HostName").FirstNode != null) singleProgram.hostname = (string)singleProgramElement.Element("HostName").FirstNode.ToString();
                    //singleProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                    if (singleProgramElement.Element("ProgramId").FirstNode != null) singleProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                    //singleProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                    //singleProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                    if (singleProgramElement.Element("EndTime").FirstNode != null)
                    {
                        DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("EndTime").FirstNode.ToString());

                        singleProgram.endtime = newEndTime.ToLocalTime().ToString("s");
                        singleProgram.endtimespace = newEndTime.ToLocalTime().ToString("s").Replace("T", " ");
                    }
                    if (singleProgramElement.Element("Airdate").FirstNode != null) singleProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                    if (singleProgramElement.Element("StartTime").FirstNode != null)
                    {
                        DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("StartTime").FirstNode.ToString());

                        singleProgram.starttime = newStartTime.ToLocalTime().ToString("s");
                        singleProgram.starttimespace = newStartTime.ToLocalTime().ToString("s").Replace("T", " ");
                    }
                    //singleProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();

                    if (singleProgramElement.Element("Channel").Element("InputId").FirstNode != null) singleProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Element("InputId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChannelName").FirstNode != null) singleProgram.channame = (string)singleProgramElement.Element("Channel").Element("ChannelName").Value;
                    if (singleProgramElement.Element("Channel").Element("SourceId").FirstNode != null) singleProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Element("SourceId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanId").FirstNode != null) singleProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Element("ChanId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanNum").FirstNode != null) singleProgram.channum = (string)singleProgramElement.Element("Channel").Element("ChanNum").Value;
                    if (singleProgramElement.Element("Channel").Element("CallSign").FirstNode != null) singleProgram.callsign = (string)singleProgramElement.Element("Channel").Element("CallSign").Value;
                    /*
                    */

                    if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                    if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                    singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                    if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                    //if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                    //if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) singleProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                    if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                    if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null)
                    {
                        DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("Recording").Element("StartTs").FirstNode.ToString());

                        singleProgram.recstartts = newStartTime.ToLocalTime().ToString("s");
                    }
                    if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null)
                    {
                        DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("Recording").Element("EndTs").FirstNode.ToString());

                        singleProgram.recendts = newEndTime.ToLocalTime().ToString("s");
                    }

                    if (singleProgramElement.Element("Artwork").Element("ArtworkInfos").FirstNode != null)
                    {
                        foreach (var singleArtworkInfoElement in singleProgramElement.Element("Artwork").Element("ArtworkInfos").Elements("ArtworkInfo"))
                        {
                            string arturlbase = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/";
                            string arturlend = "";
                            //string arturlend = "&Height=800&Width=1024";

                            switch (singleArtworkInfoElement.Element("Type").FirstNode.ToString())
                            {
                                case "coverart":
                                    singleProgram.coverart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                    break;
                                case "fanart":
                                    singleProgram.fanart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                    break;
                                case "banner":
                                    singleProgram.banner = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
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




                    if (App.ViewModel.appSettings.ChannelIconsSetting)
                        singleProgram.showChanicon = System.Windows.Visibility.Visible;
                    else
                        singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                    singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Guide/GetChannelIcon?ChanId=" + singleProgram.chanid;
                    singleProgram.screenshot = App.ViewModel.functions.CreateScreenshotUrl(singleProgram);

                    if (singleProgram.recstatus == -2)
                    {
                        singleProgram.recordedfourthline = "Currently recording (" + singleProgram.channum + " - " + singleProgram.channame + ")";
                    }
                    else
                    {
                        singleProgram.recordedfourthline = singleProgram.channum + " - " + singleProgram.channame;
                    }

                    if (singleProgram.subtitle == "") singleProgram.subtitle = ".";

                    singleProgram.description = singleProgramElement.Element("Airdate").NextNode.ToString();
                    if (singleProgramElement.Element("Description").FirstNode != null) singleProgram.description = (string)singleProgramElement.Element("Description").FirstNode.ToString();
                    if (singleProgram.description.Contains("<Inet")) singleProgram.description = "";
                    if (singleProgram.description.Contains("<Desc")) singleProgram.description = "";

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //programlist.Add(singleProgram);
                        App.ViewModel.Upcoming.Add(singleProgram);

                        /*
                        if (singleProgram.recgroup == "Default")
                        {
                            DefaultRecorded.Add(singleProgram);
                           // DefaultRecorded.OrderBy(p => p.title);
                        }
                        else if (singleProgram.recgroup == "Deleted")
                        {
                            DeletedRecorded.Add(singleProgram);
                            //DeletedRecorded.OrderBy(p => p.title);
                        }
                        else if (singleProgram.recgroup == "LiveTV")
                        {
                            LiveTVRecorded.Add(singleProgram);
                            //LiveTVRecorded.OrderBy(p => p.title);
                        }
                         */
                    });
                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getConflict25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                    webRequest.BeginGetResponse(new AsyncCallback(Conflict25Callback), webRequest);

                    //SortAndDisplay("");
                });



            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to parse recorded data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                    App.ViewModel.Connected = false;
                    NavigationService.GoBack();
                });

                return;
            }

        }
        private void Conflict25Callback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get conflict data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                    App.ViewModel.Connected = false;
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

                XDocument xdoc = XDocument.Parse(resultString, LoadOptions.None);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //App.ViewModel.Upcoming.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("ProgramList").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("ProgramList").Element("ProtoVer").Value);

                });

                //ObservableCollection<ProgramViewModel> programlist = new ObservableCollection<ProgramViewModel>();
                //int recordedCount = 0;


                foreach (XElement singleProgramElement in xdoc.Element("ProgramList").Element("Programs").Descendants("Program"))
                {
                    ProgramViewModel singleProgram = new ProgramViewModel() { };

                    if (singleProgramElement.Element("Title").FirstNode != null) singleProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                    if (singleProgramElement.Element("SubTitle").FirstNode != null) singleProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                    //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                    if (singleProgramElement.Element("Category").FirstNode != null) singleProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                    if (singleProgramElement.Element("FileSize").FirstNode != null) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                    if (singleProgramElement.Element("SeriesId").FirstNode != null) singleProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                    if (singleProgramElement.Element("HostName").FirstNode != null) singleProgram.hostname = (string)singleProgramElement.Element("HostName").FirstNode.ToString();
                    //singleProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                    if (singleProgramElement.Element("ProgramId").FirstNode != null) singleProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                    if (singleProgramElement.Element("Season").FirstNode != null) singleProgram.season = (string)singleProgramElement.Element("Season").FirstNode.ToString();
                    if (singleProgramElement.Element("Episode").FirstNode != null) singleProgram.episode = (string)singleProgramElement.Element("Episode").FirstNode.ToString();
                    //singleProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                    //singleProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                    if (singleProgramElement.Element("EndTime").FirstNode != null)
                    {
                        DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("EndTime").FirstNode.ToString());

                        singleProgram.endtime = newEndTime.ToLocalTime().ToString("s");
                        singleProgram.endtimespace = newEndTime.ToLocalTime().ToString("s").Replace("T", " ");
                    }
                    if (singleProgramElement.Element("Airdate").FirstNode != null) singleProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                    if (singleProgramElement.Element("StartTime").FirstNode != null)
                    {
                        DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("StartTime").FirstNode.ToString());

                        singleProgram.starttime = newStartTime.ToLocalTime().ToString("s");
                        singleProgram.starttimespace = newStartTime.ToLocalTime().ToString("s").Replace("T", " ");
                    }
                    //singleProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();

                    if (singleProgramElement.Element("Channel").Element("InputId").FirstNode != null) singleProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Element("InputId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChannelName").FirstNode != null) singleProgram.channame = (string)singleProgramElement.Element("Channel").Element("ChannelName").Value;
                    if (singleProgramElement.Element("Channel").Element("SourceId").FirstNode != null) singleProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Element("SourceId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanId").FirstNode != null) singleProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Element("ChanId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanNum").FirstNode != null) singleProgram.channum = (string)singleProgramElement.Element("Channel").Element("ChanNum").Value;
                    if (singleProgramElement.Element("Channel").Element("CallSign").FirstNode != null) singleProgram.callsign = (string)singleProgramElement.Element("Channel").Element("CallSign").Value;
                    /*
                    */

                    if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                    if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                    singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                    if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                    //if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                    //if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) singleProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                    if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                    if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null)
                    {
                        DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("Recording").Element("StartTs").FirstNode.ToString());

                        singleProgram.recstartts = newStartTime.ToLocalTime().ToString("s");
                    }
                    if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null)
                    {
                        DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("Recording").Element("EndTs").FirstNode.ToString());

                        singleProgram.recendts = newEndTime.ToLocalTime().ToString("s");
                    }

                    if (singleProgramElement.Element("Artwork").Element("ArtworkInfos").FirstNode != null)
                    {
                        foreach (var singleArtworkInfoElement in singleProgramElement.Element("Artwork").Element("ArtworkInfos").Elements("ArtworkInfo"))
                        {
                            string arturlbase = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/";
                            string arturlend = "";
                            //string arturlend = "&Height=800&Width=1024";

                            switch (singleArtworkInfoElement.Element("Type").FirstNode.ToString())
                            {
                                case "coverart":
                                    singleProgram.coverart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                    break;
                                case "fanart":
                                    singleProgram.fanart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                    break;
                                case "banner":
                                    singleProgram.banner = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
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




                    if (App.ViewModel.appSettings.ChannelIconsSetting)
                        singleProgram.showChanicon = System.Windows.Visibility.Visible;
                    else
                        singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                    singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Guide/GetChannelIcon?ChanId=" + singleProgram.chanid;
                    singleProgram.screenshot = App.ViewModel.functions.CreateScreenshotUrl(singleProgram);

                    if (singleProgram.recstatus == -2)
                    {
                        singleProgram.recordedfourthline = "Currently recording (" + singleProgram.channum + " - " + singleProgram.channame + ")";
                    }
                    else
                    {
                        singleProgram.recordedfourthline = singleProgram.channum + " - " + singleProgram.channame;
                    }

                    if (singleProgram.subtitle == "") singleProgram.subtitle = ".";

                    singleProgram.description = singleProgramElement.Element("Airdate").NextNode.ToString();
                    if (singleProgramElement.Element("Description").FirstNode != null) singleProgram.description = (string)singleProgramElement.Element("Description").FirstNode.ToString();
                    if (singleProgram.description.Contains("<Inet")) singleProgram.description = "";
                    if (singleProgram.description.Contains("<Desc")) singleProgram.description = "";

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //programlist.Add(singleProgram);
                        App.ViewModel.Upcoming.Add(singleProgram);

                        /*
                        if (singleProgram.recgroup == "Default")
                        {
                            DefaultRecorded.Add(singleProgram);
                           // DefaultRecorded.OrderBy(p => p.title);
                        }
                        else if (singleProgram.recgroup == "Deleted")
                        {
                            DeletedRecorded.Add(singleProgram);
                            //DeletedRecorded.OrderBy(p => p.title);
                        }
                        else if (singleProgram.recgroup == "LiveTV")
                        {
                            LiveTVRecorded.Add(singleProgram);
                            //LiveTVRecorded.OrderBy(p => p.title);
                        }
                         */
                    });
                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    SortAndDisplay("");
                });



            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to parse conflict data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                    App.ViewModel.Connected = false;
                    NavigationService.GoBack();
                });

                return;
            }

        }

        private void GetUpcomingProtocol()
        {

            protocolStatus = 0;
            //0 - not connected;
            //2 - connected
            //4 - sent MYTH_PROTO_VERSION
            //6 - got ACCEPT response
            //8 - sent ANN
            //10 - got OK
            //12 - sent command
            //14 - got initial response
            //16 - got all response data

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs sArg;
            sArg = new SocketAsyncEventArgs();

            sArg.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventCompleted);
            
            sArg.RemoteEndPoint = new DnsEndPoint(App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendPortSetting);
            sArg.UserToken = s;

            s.ConnectAsync(sArg);

        }
    
        private void SocketEventCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    protocolStatus = 2;
                    ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                default:
                    throw new Exception("Invalid operation completed");
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Connected to the server successfully
                //remoteSocket.ReceiveAsync(e);
                //Read data sent from the server
                Socket s = e.UserToken as Socket;

                byte[] buffer = Encoding.UTF8.GetBytes(App.ViewModel.functions.PadProtocol(App.ViewModel.functions.ProtoVersion()));
                e.SetBuffer(buffer, 0, buffer.Length);

                protocolStatus = 4;

                s.SendAsync(e);
                
            }
            else
            {
                //throw new SocketException((int)e.SocketError);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error connecting to backend.  Check your settings.");
                });
            }
        }
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {

                //Read data sent from the server
                Socket s = e.UserToken as Socket;

                e.SetBuffer(new Byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);
                bool willRaiseEvent = s.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                //throw new SocketException((int)e.SocketError);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error connecting to backend.  Check your settings.");
                });
            }
        }
        protected void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket s = e.UserToken as Socket;

                // Received data from server
                string newResponse = Encoding.UTF8.GetString(e.Buffer, 0, e.Buffer.Length);

                switch(protocolStatus) {
                    case 4:
                        //Sent MYTH_PROTO_VERSION, expecting ACCEPT
                        if(newResponse.Contains("ACCEPT"))
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes(App.ViewModel.functions.PadProtocol("ANN Monitor MythMe-app 0"));
                            e.SetBuffer(buffer, 0, buffer.Length);

                            protocolStatus = 8;

                            s.SendAsync(e);
                        } else 
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                MessageBox.Show("Did not get ACCEPT back: " + newResponse);
                            });
                            //throw new Exception("Did not get ACCEPT back: " + newResponse);
                        }
                        break;
                    case 8:
                        //Sent ANN, expecting OK
                        if (newResponse.Contains("OK"))
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes(App.ViewModel.functions.PadProtocol("QUERY_GETALLPENDING"));
                            e.SetBuffer(buffer, 0, buffer.Length);

                            protocolStatus = 12;
                            protocolResponseLength = 0;

                            s.SendAsync(e);
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                MessageBox.Show("Did not get OK back from ANN: " + newResponse);
                            });
                            //throw new Exception("Did not get OK back from ANN: " + newResponse);
                        }
                        break;
                    case 12:
                        protocolResponseLength = int.Parse(newResponse.Substring(0, 8).Trim());
                        fullProtocolResponse = newResponse;

                        if (fullProtocolResponse.Length < (protocolResponseLength + 8))
                        {
                            protocolStatus = 14;
                            //throw new Exception("Have some data (" + fullProtocolResponse.Length + ") but not all (" + (protocolResponseLength + 8) + ")");
                            ProcessSend(e);
                        }
                        else
                        {
                            protocolStatus = 16;

                            byte[] buffer = Encoding.UTF8.GetBytes(App.ViewModel.functions.PadProtocol("DONE"));
                            e.SetBuffer(buffer, 0, buffer.Length);

                            s.SendAsync(e);
                        }
                        break;
                    case 14:
                        fullProtocolResponse += newResponse;

                        if (fullProtocolResponse.Length < (protocolResponseLength + 8))
                        {
                            //MessageBox.Show("ready to parse upcoming");
                            ProcessSend(e);
                            //this.Perform(() => ProcessSend(e), 5);
                            //byte[] buffer = Encoding.UTF8.GetBytes(" ");
                            //e.SetBuffer(buffer, 0, buffer.Length);

                        }
                        else
                        {
                            protocolStatus = 16;

                            byte[] buffer = Encoding.UTF8.GetBytes(App.ViewModel.functions.PadProtocol("DONE"));
                            e.SetBuffer(buffer, 0, buffer.Length);

                            s.SendAsync(e);
                        }
                        break;
                    case 16:
                        //this.Perform(() => ParseUpcoming(), 100);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            ParseUpcoming();
                        });
                        break;
                    default:
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Unknown protocolStatus (" + protocolStatus + ") and response: " + newResponse);
                        });
                        //throw new Exception("Unknown protocolStatus (" + protocolStatus + ") and response: " + newResponse);
                        break;
                }

            }
            else
            {
                //throw new SocketException((int)e.SocketError);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error connecting to backend.  Check your settings.");
                });
            }
        }


        private void ParseUpcoming()
        {
            //MessageBox.Show("ready to parse upcoming of length: " + fullProtocolResponse.Length);

            if (App.ViewModel.appSettings.ProtoVerSetting == 23056)
                ParseUpcoming41();
            else if (App.ViewModel.appSettings.ProtoVerSetting >= 72)       //icon uses new URL
                ParseUpcoming72();
            else if (App.ViewModel.appSettings.ProtoVerSetting >= 67)
                ParseUpcoming67();
            else if (App.ViewModel.appSettings.ProtoVerSetting >= 57)
                ParseUpcoming57();
            else if (App.ViewModel.appSettings.ProtoVerSetting >= 41)
                ParseUpcoming41();
            else if (App.ViewModel.appSettings.ProtoVerSetting >= 35)
                ParseUpcoming35();
            else if (App.ViewModel.appSettings.ProtoVerSetting >= 32)
                ParseUpcoming32();
            else if (App.ViewModel.appSettings.ProtoVerSetting >= 31)
                ParseUpcoming31();
            else if (App.ViewModel.appSettings.ProtoVerSetting >= 25)
                ParseUpcoming25();
            else
                MessageBox.Show("Your MythTV protocol version ("+App.ViewModel.appSettings.ProtoVerSetting+") is old and is not supported by MythMe");
        }
        private void ParseUpcoming72()
        {

            //Protocol version 67 and up - 44 fields

            App.ViewModel.Upcoming.Clear();

            string[] stringSeparators = new string[] { "[]:[]" };
            string[] responseArray;

            ProgramViewModel singleProgram = new ProgramViewModel();
            //int arrayIndex = 0;
            int programIndex = 0;
            int fieldIndex = 0;
            int i;

            int offset = 2; //upcoming is conflicting[]:[]qty programs[]:[]programs ...

            responseArray = fullProtocolResponse.Split(stringSeparators, StringSplitOptions.None);

            try
            {
                for (i = offset; i < responseArray.Length; i++)
                {
                    switch (fieldIndex)
                    {
                        case 0:
                            singleProgram = new ProgramViewModel();
                            singleProgram.title = responseArray[i];
                            break;
                        case 1:
                            singleProgram.subtitle = responseArray[i];
                            break;
                        case 2:
                            singleProgram.description = responseArray[i];
                            break;
                        case 3:
                            singleProgram.season = responseArray[i];
                            break;
                        case 4:
                            singleProgram.episode = responseArray[i];
                            break;
                        case 5:
                            singleProgram.category = responseArray[i];
                            break;
                        case 6:
                            singleProgram.chanid = int.Parse(responseArray[i]);
                            singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Guide/GetChannelIcon?ChanId=" + responseArray[i];
                            break;
                        case 7:
                            singleProgram.channum = responseArray[i];
                            break;
                        case 8:
                            singleProgram.callsign = responseArray[i];
                            break;
                        case 9:
                            singleProgram.channame = responseArray[i];
                            break;

                        case 10:
                            singleProgram.filename = responseArray[i];
                            break;
                        case 11:
                            //singleProgram.filesize = Int64.Parse(responseArray[i]);
                            break;
                        case 12:
                            singleProgram.starttimeint = Int64.Parse(responseArray[i]);
                            DateTime st = new DateTime(1970, 1, 1).AddSeconds(singleProgram.starttimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //st.Add(new TimeSpan(singleProgram.starttimeint * 10000000));
                            //st.AddSeconds(singleProgram.starttimeint);
                            singleProgram.starttime = st.ToString("s");
                            singleProgram.starttimespace = singleProgram.starttime.Replace("T", " ");
                            break;
                        case 13:
                            singleProgram.endtimeint = Int64.Parse(responseArray[i]);
                            DateTime et = new DateTime(1970, 1, 1).AddSeconds(singleProgram.endtimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //et.Add(new TimeSpan(singleProgram.endtimeint*10000000));
                            //et.AddSeconds(singleProgram.endtimeint);
                            singleProgram.endtime = et.ToString("s");
                            //asdf
                            break;
                        case 14:
                            singleProgram.findid = responseArray[i];
                            break;
                        case 15:
                            singleProgram.hostname = responseArray[i];
                            break;
                        case 16:
                            singleProgram.sourceid = int.Parse(responseArray[i]);
                            break;
                        case 17:
                            singleProgram.cardid = int.Parse(responseArray[i]);
                            break;
                        case 18:
                            singleProgram.inputid = int.Parse(responseArray[i]);
                            break;
                        case 19:
                            singleProgram.recpriority = int.Parse(responseArray[i]);
                            break;

                        case 20:
                            singleProgram.recstatus = int.Parse(responseArray[i]);
                            singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //singleProgram.recstatustext = responseArray[i];
                            break;
                        case 21:
                            singleProgram.recordid = int.Parse(responseArray[i]);
                            break;
                        case 22:
                            singleProgram.rectype = int.Parse(responseArray[i]);
                            break;
                        case 23:
                            //singleProgram.dupin = responseArray[i];
                            break;
                        case 24:
                            //singleProgram.dupmethod = responseArray[i];
                            break;
                        case 25:
                            singleProgram.recstarttsint = Int64.Parse(responseArray[i]);
                            DateTime rt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recstarttsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //rt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //rt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recstartts = rt.ToString("s");
                            break;
                        case 26:
                            singleProgram.recendtsint = Int64.Parse(responseArray[i]);
                            DateTime tt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recendtsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //tt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //tt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recendts = tt.ToString("s");
                            //asdf
                            break;
                        case 27:
                            //singleProgram.programflags = int.Parse(responseArray[i]);
                            break;
                        case 28:
                            singleProgram.recgroup = responseArray[i];
                            break;
                        case 29:
                            //singleProgram.outputfilters = responseArray[i];
                            break;

                        case 30:
                            singleProgram.seriesid = responseArray[i];
                            break;
                        case 31:
                            singleProgram.programid = responseArray[i];
                            break;
                        case 32:
                            //singleProgram.initref = responseArray[i];
                            break;
                        case 33:
                            singleProgram.lastmodified = responseArray[i];
                            break;
                        case 34:
                            //singleProgram.stars = responseArray[i];
                            break;
                        case 35:
                            singleProgram.airdate = responseArray[i];
                            break;
                        case 36:
                            singleProgram.playgroup = responseArray[i];
                            break;
                        case 37:
                            singleProgram.recpriority2 = responseArray[i];
                            break;
                        case 38:
                            singleProgram.parentid = responseArray[i];
                            break;
                        case 39:
                            singleProgram.storagegroup = responseArray[i];
                            break;

                        case 40:
                            //singleProgram.audio_props = responseArray[i];
                            break;
                        case 41:
                            //singleProgram.video_props = responseArray[i];
                            break;
                        case 42:
                            //singleProgram.subtitle_type = responseArray[i];
                            break;
                        //
                        case 43:
                            //singleProgram.year = responseArray[i];

                            if (App.ViewModel.appSettings.ChannelIconsSetting)
                                singleProgram.showChanicon = System.Windows.Visibility.Visible;
                            else
                                singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                            App.ViewModel.Upcoming.Add(singleProgram);
                            programIndex++;
                            fieldIndex = -1;
                            break;
                    }

                    fieldIndex++;

                }

                //MessageBox.Show("Finished parsing programs: " + App.ViewModel.Upcoming.Count + " view model, " + programIndex + " program index, " + responseArray[1] + " protocol");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible.  There should have been " + responseArray[1] + " scheduled recordings listed instead of just " + App.ViewModel.Upcoming.Count + ".", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());
        }
        private void ParseUpcoming67()
        {

            //Protocol version 67 and up - 44 fields

            App.ViewModel.Upcoming.Clear();

            string[] stringSeparators = new string[] { "[]:[]" };
            string[] responseArray;

            ProgramViewModel singleProgram = new ProgramViewModel();
            //int arrayIndex = 0;
            int programIndex = 0;
            int fieldIndex = 0;
            int i;

            int offset = 2; //upcoming is conflicting[]:[]qty programs[]:[]programs ...

            responseArray = fullProtocolResponse.Split(stringSeparators, StringSplitOptions.None);

            try
            {
                for (i = offset; i < responseArray.Length; i++)
                {
                    switch (fieldIndex)
                    {
                        case 0:
                            singleProgram = new ProgramViewModel();
                            singleProgram.title = responseArray[i];
                            break;
                        case 1:
                            singleProgram.subtitle = responseArray[i];
                            break;
                        case 2:
                            singleProgram.description = responseArray[i];
                            break;
                        case 3:
                            singleProgram.season = responseArray[i];
                            break;
                        case 4:
                            singleProgram.episode = responseArray[i];
                            break;
                        case 5:
                            singleProgram.category = responseArray[i];
                            break;
                        case 6:
                            singleProgram.chanid = int.Parse(responseArray[i]);
                            singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + responseArray[i];
                            break;
                        case 7:
                            singleProgram.channum = responseArray[i];
                            break;
                        case 8:
                            singleProgram.callsign = responseArray[i];
                            break;
                        case 9:
                            singleProgram.channame = responseArray[i];
                            break;

                        case 10:
                            singleProgram.filename = responseArray[i];
                            break;
                        case 11:
                            //singleProgram.filesize = Int64.Parse(responseArray[i]);
                            break;
                        case 12:
                            singleProgram.starttimeint = Int64.Parse(responseArray[i]);
                            DateTime st = new DateTime(1970, 1, 1).AddSeconds(singleProgram.starttimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //st.Add(new TimeSpan(singleProgram.starttimeint * 10000000));
                            //st.AddSeconds(singleProgram.starttimeint);
                            singleProgram.starttime = st.ToString("s");
                            singleProgram.starttimespace = singleProgram.starttime.Replace("T", " ");
                            break;
                        case 13:
                            singleProgram.endtimeint = Int64.Parse(responseArray[i]);
                            DateTime et = new DateTime(1970, 1, 1).AddSeconds(singleProgram.endtimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //et.Add(new TimeSpan(singleProgram.endtimeint*10000000));
                            //et.AddSeconds(singleProgram.endtimeint);
                            singleProgram.endtime = et.ToString("s");
                            //asdf
                            break;
                        case 14:
                            singleProgram.findid = responseArray[i];
                            break;
                        case 15:
                            singleProgram.hostname = responseArray[i];
                            break;
                        case 16:
                            singleProgram.sourceid = int.Parse(responseArray[i]);
                            break;
                        case 17:
                            singleProgram.cardid = int.Parse(responseArray[i]);
                            break;
                        case 18:
                            singleProgram.inputid = int.Parse(responseArray[i]);
                            break;
                        case 19:
                            singleProgram.recpriority = int.Parse(responseArray[i]);
                            break;

                        case 20:
                            singleProgram.recstatus = int.Parse(responseArray[i]);
                            singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //singleProgram.recstatustext = responseArray[i];
                            break;
                        case 21:
                            singleProgram.recordid = int.Parse(responseArray[i]);
                            break;
                        case 22:
                            singleProgram.rectype = int.Parse(responseArray[i]);
                            break;
                        case 23:
                            //singleProgram.dupin = responseArray[i];
                            break;
                        case 24:
                            //singleProgram.dupmethod = responseArray[i];
                            break;
                        case 25:
                            singleProgram.recstarttsint = Int64.Parse(responseArray[i]);
                            DateTime rt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recstarttsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //rt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //rt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recstartts = rt.ToString("s");
                            break;
                        case 26:
                            singleProgram.recendtsint = Int64.Parse(responseArray[i]);
                            DateTime tt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recendtsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //tt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //tt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recendts = tt.ToString("s");
                            //asdf
                            break;
                        case 27:
                            //singleProgram.programflags = int.Parse(responseArray[i]);
                            break;
                        case 28:
                            singleProgram.recgroup = responseArray[i];
                            break;
                        case 29:
                            //singleProgram.outputfilters = responseArray[i];
                            break;

                        case 30:
                            singleProgram.seriesid = responseArray[i];
                            break;
                        case 31:
                            singleProgram.programid = responseArray[i];
                            break;
                        case 32:
                            //singleProgram.initref = responseArray[i];
                            break;
                        case 33:
                            singleProgram.lastmodified = responseArray[i];
                            break;
                        case 34:
                            //singleProgram.stars = responseArray[i];
                            break;
                        case 35:
                            singleProgram.airdate = responseArray[i];
                            break;
                        case 36:
                            singleProgram.playgroup = responseArray[i];
                            break;
                        case 37:
                            singleProgram.recpriority2 = responseArray[i];
                            break;
                        case 38:
                            singleProgram.parentid = responseArray[i];
                            break;
                        case 39:
                            singleProgram.storagegroup = responseArray[i];
                            break;

                        case 40:
                            //singleProgram.audio_props = responseArray[i];
                            break;
                        case 41:
                            //singleProgram.video_props = responseArray[i];
                            break;
                        case 42:
                            //singleProgram.subtitle_type = responseArray[i];
                            break;
                        //
                        case 43:
                            //singleProgram.year = responseArray[i];

                            if (App.ViewModel.appSettings.ChannelIconsSetting)
                                singleProgram.showChanicon = System.Windows.Visibility.Visible;
                            else
                                singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                            App.ViewModel.Upcoming.Add(singleProgram);
                            programIndex++;
                            fieldIndex = -1;
                            break;
                    }

                    fieldIndex++;

                }

                //MessageBox.Show("Finished parsing programs: " + App.ViewModel.Upcoming.Count + " view model, " + programIndex + " program index, " + responseArray[1] + " protocol");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible.  There should have been " + responseArray[1] + " scheduled recordings listed instead of just " + App.ViewModel.Upcoming.Count + ".", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());
        }
        private void ParseUpcoming57()
        {
            
	        //Protocol version 57 and up - 41 fields

            App.ViewModel.Upcoming.Clear();

            string[] stringSeparators = new string[] {"[]:[]"};
            string[] responseArray;

            ProgramViewModel singleProgram = new ProgramViewModel();
            //int arrayIndex = 0;
            int programIndex = 0;
            int fieldIndex = 0;
            int i;

            int offset = 2; //upcoming is conflicting[]:[]qty programs[]:[]programs ...

            responseArray = fullProtocolResponse.Split(stringSeparators, StringSplitOptions.None);

            try
            {
                for (i = offset; i < responseArray.Length; i++)
                {
                    switch (fieldIndex)
                    {
                        case 0:
                            singleProgram = new ProgramViewModel(); 
                            singleProgram.title = responseArray[i];
                            break;
                        case 1:
                            singleProgram.subtitle = responseArray[i];
                            break;
                        case 2:
                            singleProgram.description = responseArray[i];
                            break;
                        case 3:
                            singleProgram.category = responseArray[i];
                            break;
                        case 4:
                            singleProgram.chanid = int.Parse(responseArray[i]);
                            singleProgram.chanicon = "http://"+App.ViewModel.appSettings.MasterBackendIpSetting+":"+App.ViewModel.appSettings.MasterBackendXmlPortSetting+"/Myth/GetChannelIcon?ChanId="+responseArray[i];
                            break;
                        case 5:
                            singleProgram.channum = responseArray[i];
                            break;
                        case 6:
                            singleProgram.callsign = responseArray[i];
                            break;
                        case 7:
                            singleProgram.channame = responseArray[i];
                            break;
                        case 8:
                            singleProgram.filename = responseArray[i];
                            break;
                        case 9:
                            //singleProgram.filesize = Int64.Parse(responseArray[i]);
                            break;

                        case 10:
                            singleProgram.starttimeint = Int64.Parse(responseArray[i]);
                            DateTime st = new DateTime(1970, 1, 1).AddSeconds(singleProgram.starttimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //st.Add(new TimeSpan(singleProgram.starttimeint * 10000000));
                            //st.AddSeconds(singleProgram.starttimeint);
                            singleProgram.starttime = st.ToString("s");
                            singleProgram.starttimespace = singleProgram.starttime.Replace("T", " ");
                            break;
                        case 11:
                            singleProgram.endtimeint = Int64.Parse(responseArray[i]);
                            DateTime et = new DateTime(1970, 1, 1).AddSeconds(singleProgram.endtimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //et.Add(new TimeSpan(singleProgram.endtimeint*10000000));
                            //et.AddSeconds(singleProgram.endtimeint);
                            singleProgram.endtime = et.ToString("s");
                            //asdf
                            break;
                        case 12:
                            singleProgram.findid = responseArray[i];
                            break;
                        case 13:
                            singleProgram.hostname = responseArray[i];
                            break;
                        case 14:
                            singleProgram.sourceid = int.Parse(responseArray[i]);
                            break;
                        case 15:
                            singleProgram.cardid = int.Parse(responseArray[i]);
                            break;
                        case 16:
                            singleProgram.inputid = int.Parse(responseArray[i]);
                            break;
                        case 17:
                            singleProgram.recpriority = int.Parse(responseArray[i]);
                            break;
                        case 18:
                            singleProgram.recstatus = int.Parse(responseArray[i]);
                            singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //singleProgram.recstatustext = responseArray[i];
                            break;
                        case 19:
                            singleProgram.recordid = int.Parse(responseArray[i]);
                            break;

                        case 20:
                            singleProgram.rectype = int.Parse(responseArray[i]);
                            break;
                        case 21:
                            //singleProgram.dupin = responseArray[i];
                            break;
                        case 22:
                            //singleProgram.dupmethod = responseArray[i];
                            break;
                        case 23:
                            singleProgram.recstarttsint = Int64.Parse(responseArray[i]);
                            DateTime rt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recstarttsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //rt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //rt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recstartts = rt.ToString("s");
                            break;
                        case 24:
                            singleProgram.recendtsint = Int64.Parse(responseArray[i]);
                            DateTime tt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recendtsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //tt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //tt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recendts = tt.ToString("s");
                            //asdf
                            break;
                        case 25:
                            //singleProgram.programflags = int.Parse(responseArray[i]);
                            break;
                        case 26:
                            singleProgram.recgroup = responseArray[i];
                            break;
                        case 27:
                            //singleProgram.outputfilters = responseArray[i];
                            break;
                        case 28:
                            singleProgram.seriesid = responseArray[i];
                            break;
                        case 29:
                            singleProgram.programid = responseArray[i];
                            break;

                        case 30:
                            singleProgram.lastmodified = responseArray[i];
                            break;
                        case 31:
                            //singleProgram.stars = responseArray[i];
                            break;
                        case 32:
                            singleProgram.airdate = responseArray[i];
                            break;
                        case 33:
                            singleProgram.playgroup = responseArray[i];
                            break;
                        case 34:
                            singleProgram.recpriority2 = responseArray[i];
                            break;
                        case 35:
                            singleProgram.parentid = responseArray[i];
                            break;
                        case 36:
                            singleProgram.storagegroup = responseArray[i];
                            break;
                        case 37:
                            //singleProgram.audio_props = responseArray[i];
                            break;
                        case 38:
                            //singleProgram.video_props = responseArray[i];
                            break;
                        case 39:
                            //singleProgram.subtitle_type = responseArray[i];
                            break;

                        //
                        case 40:
                            //singleProgram.year = responseArray[i];

                            if (App.ViewModel.appSettings.ChannelIconsSetting)
                                singleProgram.showChanicon = System.Windows.Visibility.Visible;
                            else
                                singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                            App.ViewModel.Upcoming.Add(singleProgram);
                            programIndex++;
                            fieldIndex = -1;
                            break;
                    }

                    fieldIndex++;

                }

                //MessageBox.Show("Finished parsing programs: " + App.ViewModel.Upcoming.Count + " view model, " + programIndex + " program index, " + responseArray[1] + " protocol");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible.  There should have been "+responseArray[1]+" scheduled recordings listed instead of just "+App.ViewModel.Upcoming.Count+".", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());

        }
        private void ParseUpcoming41()
        {

            //Protocol version 41 and up - 47 fields

            App.ViewModel.Upcoming.Clear();

            string[] stringSeparators = new string[] { "[]:[]" };
            string[] responseArray;

            ProgramViewModel singleProgram = new ProgramViewModel();
            //int arrayIndex = 0;
            int programIndex = 0;
            int fieldIndex = 0;
            int i;

            int offset = 2; //upcoming is conflicting[]:[]qty programs[]:[]programs ...

            responseArray = fullProtocolResponse.Split(stringSeparators, StringSplitOptions.None);

            try
            {
                for (i = offset; i < responseArray.Length; i++)
                {
                    switch (fieldIndex)
                    {
                        case 0:
                            singleProgram = new ProgramViewModel();
                            singleProgram.title = responseArray[i];
                            break;
                        case 1:
                            singleProgram.subtitle = responseArray[i];
                            break;
                        case 2:
                            singleProgram.description = responseArray[i];
                            break;
                        case 3:
                            singleProgram.category = responseArray[i];
                            break;
                        case 4:
                            singleProgram.chanid = int.Parse(responseArray[i]);
                            singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + responseArray[i];
                            break;
                        case 5:
                            singleProgram.channum = responseArray[i];
                            break;
                        case 6:
                            singleProgram.callsign = responseArray[i];
                            break;
                        case 7:
                            singleProgram.channame = responseArray[i];
                            break;
                        case 8:
                            singleProgram.filename = responseArray[i];
                            break;
                        case 9:
                            //singleProgram.fs_high = Int64.Parse(responseArray[i]);
                            break;
                        case 10:
                            //singleProgram.fs_low = Int64.Parse(responseArray[i]);
                            break;

                        case 11:
                            singleProgram.starttimeint = Int64.Parse(responseArray[i]);
                            DateTime st = new DateTime(1970, 1, 1).AddSeconds(singleProgram.starttimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //st.Add(new TimeSpan(singleProgram.starttimeint * 10000000));
                            //st.AddSeconds(singleProgram.starttimeint);
                            singleProgram.starttime = st.ToString("s");
                            singleProgram.starttimespace = singleProgram.starttime.Replace("T", " ");
                            break;
                        case 12:
                            singleProgram.endtimeint = Int64.Parse(responseArray[i]);
                            DateTime et = new DateTime(1970, 1, 1).AddSeconds(singleProgram.endtimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //et.Add(new TimeSpan(singleProgram.endtimeint*10000000));
                            //et.AddSeconds(singleProgram.endtimeint);
                            singleProgram.endtime = et.ToString("s");
                            //asdf
                            break;
                        case 13:
                            //singleProgram.duplicate = responseArray[i];
                            break;
                        case 14:
                            //singleProgram.shareable = responseArray[i];
                            break;
                        case 15:
                            singleProgram.findid = responseArray[i];
                            break;
                        case 16:
                            singleProgram.hostname = responseArray[i];
                            break;
                        case 17:
                            singleProgram.sourceid = int.Parse(responseArray[i]);
                            break;
                        case 18:
                            singleProgram.cardid = int.Parse(responseArray[i]);
                            break;
                        case 19:
                            singleProgram.inputid = int.Parse(responseArray[i]);
                            break;

                        case 20:
                            singleProgram.recpriority = int.Parse(responseArray[i]);
                            break;
                        case 21:
                            singleProgram.recstatus = int.Parse(responseArray[i]);
                            singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //singleProgram.recstatustext = responseArray[i];
                            break;
                        case 22:
                            singleProgram.recordid = int.Parse(responseArray[i]);
                            break;
                        case 23:
                            singleProgram.rectype = int.Parse(responseArray[i]);
                            break;
                        case 24:
                            //singleProgram.dupin = responseArray[i];
                            break;
                        case 25:
                            //singleProgram.dupmethod = responseArray[i];
                            break;
                        case 26:
                            singleProgram.recstarttsint = Int64.Parse(responseArray[i]);
                            DateTime rt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recstarttsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //rt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //rt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recstartts = rt.ToString("s");
                            break;
                        case 27:
                            singleProgram.recendtsint = Int64.Parse(responseArray[i]);
                            DateTime tt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recendtsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //tt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //tt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recendts = tt.ToString("s");
                            //asdf
                            break;
                        case 28:
                            //singleProgram.repeat = int.Parse(responseArray[i]);
                            break;
                        case 29:
                            //singleProgram.programflags = int.Parse(responseArray[i]);
                            break;

                        case 30:
                            singleProgram.recgroup = responseArray[i];
                            break;
                        case 31:
                            //singleProgram.chancommfree = responseArray[i];
                            break;
                        case 32:
                            //singleProgram.outputfilters = responseArray[i];
                            break;
                        case 33:
                            singleProgram.seriesid = responseArray[i];
                            break;
                        case 34:
                            singleProgram.programid = responseArray[i];
                            break;
                        case 35:
                            singleProgram.lastmodified = responseArray[i];
                            break;
                        case 36:
                            //singleProgram.stars = responseArray[i];
                            break;
                        case 37:
                            singleProgram.airdate = responseArray[i];
                            break;
                        case 38:
                            //singleProgram.hasairdate = responseArray[i];
                            break;
                        case 39:
                            singleProgram.playgroup = responseArray[i];
                            break;

                        case 40:
                            singleProgram.recpriority2 = responseArray[i];
                            break;
                        case 41:
                            singleProgram.parentid = responseArray[i];
                            break;
                        case 42:
                            singleProgram.storagegroup = responseArray[i];
                            break;
                        case 43:
                            //singleProgram.audio_props = responseArray[i];
                            break;
                        case 44:
                            //singleProgram.video_props = responseArray[i];
                            break;
                        case 45:
                            //singleProgram.subtitle_type = responseArray[i];
                            break;
                        case 46:
                            //singleProgram.year = responseArray[i];

                            if (App.ViewModel.appSettings.ChannelIconsSetting)
                                singleProgram.showChanicon = System.Windows.Visibility.Visible;
                            else
                                singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                            App.ViewModel.Upcoming.Add(singleProgram);
                            programIndex++;
                            fieldIndex = -1;
                            break;
                    }

                    fieldIndex++;

                }

                //MessageBox.Show("Finished parsing programs: " + App.ViewModel.Upcoming.Count + " view model, " + programIndex + " program index, " + responseArray[1] + " protocol");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible.  There should have been " + responseArray[1] + " scheduled recordings listed instead of just " + App.ViewModel.Upcoming.Count + ".", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());

        }
        private void ParseUpcoming35()
        {
            //Protocol version 35 and up - 46 fields

            App.ViewModel.Upcoming.Clear();

            string[] stringSeparators = new string[] { "[]:[]" };
            string[] responseArray;

            ProgramViewModel singleProgram = new ProgramViewModel();
            //int arrayIndex = 0;
            int programIndex = 0;
            int fieldIndex = 0;
            int i;

            int offset = 2; //upcoming is conflicting[]:[]qty programs[]:[]programs ...

            responseArray = fullProtocolResponse.Split(stringSeparators, StringSplitOptions.None);

            try
            {
                for (i = offset; i < responseArray.Length; i++)
                {
                    switch (fieldIndex)
                    {
                        case 0:
                            singleProgram = new ProgramViewModel();
                            singleProgram.title = responseArray[i];
                            break;
                        case 1:
                            singleProgram.subtitle = responseArray[i];
                            break;
                        case 2:
                            singleProgram.description = responseArray[i];
                            break;
                        case 3:
                            singleProgram.category = responseArray[i];
                            break;
                        case 4:
                            singleProgram.chanid = int.Parse(responseArray[i]);
                            singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + responseArray[i];
                            break;
                        case 5:
                            singleProgram.channum = responseArray[i];
                            break;
                        case 6:
                            singleProgram.callsign = responseArray[i];
                            break;
                        case 7:
                            singleProgram.channame = responseArray[i];
                            break;
                        case 8:
                            singleProgram.filename = responseArray[i];
                            break;
                        case 9:
                            //singleProgram.fs_high = Int64.Parse(responseArray[i]);
                            break;
                        case 10:
                            //singleProgram.fs_low = Int64.Parse(responseArray[i]);
                            break;

                        case 11:
                            singleProgram.starttimeint = Int64.Parse(responseArray[i]);
                            DateTime st = new DateTime(1970, 1, 1).AddSeconds(singleProgram.starttimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //st.Add(new TimeSpan(singleProgram.starttimeint * 10000000));
                            //st.AddSeconds(singleProgram.starttimeint);
                            singleProgram.starttime = st.ToString("s");
                            singleProgram.starttimespace = singleProgram.starttime.Replace("T", " ");
                            break;
                        case 12:
                            singleProgram.endtimeint = Int64.Parse(responseArray[i]);
                            DateTime et = new DateTime(1970, 1, 1).AddSeconds(singleProgram.endtimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //et.Add(new TimeSpan(singleProgram.endtimeint*10000000));
                            //et.AddSeconds(singleProgram.endtimeint);
                            singleProgram.endtime = et.ToString("s");
                            //asdf
                            break;
                        case 13:
                            //singleProgram.duplicate = responseArray[i];
                            break;
                        case 14:
                            //singleProgram.shareable = responseArray[i];
                            break;
                        case 15:
                            singleProgram.findid = responseArray[i];
                            break;
                        case 16:
                            singleProgram.hostname = responseArray[i];
                            break;
                        case 17:
                            singleProgram.sourceid = int.Parse(responseArray[i]);
                            break;
                        case 18:
                            singleProgram.cardid = int.Parse(responseArray[i]);
                            break;
                        case 19:
                            singleProgram.inputid = int.Parse(responseArray[i]);
                            break;

                        case 20:
                            singleProgram.recpriority = int.Parse(responseArray[i]);
                            break;
                        case 21:
                            singleProgram.recstatus = int.Parse(responseArray[i]);
                            singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //singleProgram.recstatustext = responseArray[i];
                            break;
                        case 22:
                            singleProgram.recordid = int.Parse(responseArray[i]);
                            break;
                        case 23:
                            singleProgram.rectype = int.Parse(responseArray[i]);
                            break;
                        case 24:
                            //singleProgram.dupin = responseArray[i];
                            break;
                        case 25:
                            //singleProgram.dupmethod = responseArray[i];
                            break;
                        case 26:
                            singleProgram.recstarttsint = Int64.Parse(responseArray[i]);
                            DateTime rt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recstarttsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //rt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //rt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recstartts = rt.ToString("s");
                            break;
                        case 27:
                            singleProgram.recendtsint = Int64.Parse(responseArray[i]);
                            DateTime tt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recendtsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //tt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //tt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recendts = tt.ToString("s");
                            //asdf
                            break;
                        case 28:
                            //singleProgram.repeat = int.Parse(responseArray[i]);
                            break;
                        case 29:
                            //singleProgram.programflags = int.Parse(responseArray[i]);
                            break;

                        case 30:
                            singleProgram.recgroup = responseArray[i];
                            break;
                        case 31:
                            //singleProgram.chancommfree = responseArray[i];
                            break;
                        case 32:
                            //singleProgram.outputfilters = responseArray[i];
                            break;
                        case 33:
                            singleProgram.seriesid = responseArray[i];
                            break;
                        case 34:
                            singleProgram.programid = responseArray[i];
                            break;
                        case 35:
                            singleProgram.lastmodified = responseArray[i];
                            break;
                        case 36:
                            //singleProgram.stars = responseArray[i];
                            break;
                        case 37:
                            singleProgram.airdate = responseArray[i];
                            break;
                        case 38:
                            //singleProgram.hasairdate = responseArray[i];
                            break;
                        case 39:
                            singleProgram.playgroup = responseArray[i];
                            break;

                        case 40:
                            singleProgram.recpriority2 = responseArray[i];
                            break;
                        case 41:
                            singleProgram.parentid = responseArray[i];
                            break;
                        case 42:
                            singleProgram.storagegroup = responseArray[i];
                            break;
                        case 43:
                            //singleProgram.audio_props = responseArray[i];
                            break;
                        case 44:
                            //singleProgram.video_props = responseArray[i];
                            break;
                        case 45:
                            //singleProgram.subtitle_type = responseArray[i];

                            if (App.ViewModel.appSettings.ChannelIconsSetting)
                                singleProgram.showChanicon = System.Windows.Visibility.Visible;
                            else
                                singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                            App.ViewModel.Upcoming.Add(singleProgram);
                            programIndex++;
                            fieldIndex = -1;
                            break;
                    }

                    fieldIndex++;

                }

                //MessageBox.Show("Finished parsing programs: " + App.ViewModel.Upcoming.Count + " view model, " + programIndex + " program index, " + responseArray[1] + " protocol");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible.  There should have been " + responseArray[1] + " scheduled recordings listed instead of just " + App.ViewModel.Upcoming.Count + ".", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());
        }
        private void ParseUpcoming32()
        {

            //Protocol version 32 and up - 43 fields

            App.ViewModel.Upcoming.Clear();

            string[] stringSeparators = new string[] { "[]:[]" };
            string[] responseArray;

            ProgramViewModel singleProgram = new ProgramViewModel();
            //int arrayIndex = 0;
            int programIndex = 0;
            int fieldIndex = 0;
            int i;

            int offset = 2; //upcoming is conflicting[]:[]qty programs[]:[]programs ...

            responseArray = fullProtocolResponse.Split(stringSeparators, StringSplitOptions.None);

            try
            {
                for (i = offset; i < responseArray.Length; i++)
                {
                    switch (fieldIndex)
                    {
                        case 0:
                            singleProgram = new ProgramViewModel();
                            singleProgram.title = responseArray[i];
                            break;
                        case 1:
                            singleProgram.subtitle = responseArray[i];
                            break;
                        case 2:
                            singleProgram.description = responseArray[i];
                            break;
                        case 3:
                            singleProgram.category = responseArray[i];
                            break;
                        case 4:
                            singleProgram.chanid = int.Parse(responseArray[i]);
                            singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + responseArray[i];
                            break;
                        case 5:
                            singleProgram.channum = responseArray[i];
                            break;
                        case 6:
                            singleProgram.callsign = responseArray[i];
                            break;
                        case 7:
                            singleProgram.channame = responseArray[i];
                            break;
                        case 8:
                            singleProgram.filename = responseArray[i];
                            break;
                        case 9:
                            //singleProgram.fs_high = Int64.Parse(responseArray[i]);
                            break;
                        case 10:
                            //singleProgram.fs_low = Int64.Parse(responseArray[i]);
                            break;

                        case 11:
                            singleProgram.starttimeint = Int64.Parse(responseArray[i]);
                            DateTime st = new DateTime(1970, 1, 1).AddSeconds(singleProgram.starttimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //st.Add(new TimeSpan(singleProgram.starttimeint * 10000000));
                            //st.AddSeconds(singleProgram.starttimeint);
                            singleProgram.starttime = st.ToString("s");
                            singleProgram.starttimespace = singleProgram.starttime.Replace("T", " ");
                            break;
                        case 12:
                            singleProgram.endtimeint = Int64.Parse(responseArray[i]);
                            DateTime et = new DateTime(1970, 1, 1).AddSeconds(singleProgram.endtimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //et.Add(new TimeSpan(singleProgram.endtimeint*10000000));
                            //et.AddSeconds(singleProgram.endtimeint);
                            singleProgram.endtime = et.ToString("s");
                            //asdf
                            break;
                        case 13:
                            //singleProgram.duplicate = responseArray[i];
                            break;
                        case 14:
                            //singleProgram.shareable = responseArray[i];
                            break;
                        case 15:
                            singleProgram.findid = responseArray[i];
                            break;
                        case 16:
                            singleProgram.hostname = responseArray[i];
                            break;
                        case 17:
                            singleProgram.sourceid = int.Parse(responseArray[i]);
                            break;
                        case 18:
                            singleProgram.cardid = int.Parse(responseArray[i]);
                            break;
                        case 19:
                            singleProgram.inputid = int.Parse(responseArray[i]);
                            break;

                        case 20:
                            singleProgram.recpriority = int.Parse(responseArray[i]);
                            break;
                        case 21:
                            singleProgram.recstatus = int.Parse(responseArray[i]);
                            singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //singleProgram.recstatustext = responseArray[i];
                            break;
                        case 22:
                            singleProgram.recordid = int.Parse(responseArray[i]);
                            break;
                        case 23:
                            singleProgram.rectype = int.Parse(responseArray[i]);
                            break;
                        case 24:
                            //singleProgram.dupin = responseArray[i];
                            break;
                        case 25:
                            //singleProgram.dupmethod = responseArray[i];
                            break;
                        case 26:
                            singleProgram.recstarttsint = Int64.Parse(responseArray[i]);
                            DateTime rt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recstarttsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //rt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //rt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recstartts = rt.ToString("s");
                            break;
                        case 27:
                            singleProgram.recendtsint = Int64.Parse(responseArray[i]);
                            DateTime tt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recendtsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //tt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //tt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recendts = tt.ToString("s");
                            //asdf
                            break;
                        case 28:
                            //singleProgram.repeat = int.Parse(responseArray[i]);
                            break;
                        case 29:
                            //singleProgram.programflags = int.Parse(responseArray[i]);
                            break;

                        case 30:
                            singleProgram.recgroup = responseArray[i];
                            break;
                        case 31:
                            //singleProgram.chancommfree = responseArray[i];
                            break;
                        case 32:
                            //singleProgram.outputfilters = responseArray[i];
                            break;
                        case 33:
                            singleProgram.seriesid = responseArray[i];
                            break;
                        case 34:
                            singleProgram.programid = responseArray[i];
                            break;
                        case 35:
                            singleProgram.lastmodified = responseArray[i];
                            break;
                        case 36:
                            //singleProgram.stars = responseArray[i];
                            break;
                        case 37:
                            singleProgram.airdate = responseArray[i];
                            break;
                        case 38:
                            //singleProgram.hasairdate = responseArray[i];
                            break;
                        case 39:
                            singleProgram.playgroup = responseArray[i];
                            break;

                        case 40:
                            singleProgram.recpriority2 = responseArray[i];
                            break;
                        case 41:
                            singleProgram.parentid = responseArray[i];
                            break;
                        case 42:
                            singleProgram.storagegroup = responseArray[i];

                            if (App.ViewModel.appSettings.ChannelIconsSetting)
                                singleProgram.showChanicon = System.Windows.Visibility.Visible;
                            else
                                singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                            App.ViewModel.Upcoming.Add(singleProgram);
                            programIndex++;
                            fieldIndex = -1;
                            break;
                    }

                    fieldIndex++;

                }

                //MessageBox.Show("Finished parsing programs: " + App.ViewModel.Upcoming.Count + " view model, " + programIndex + " program index, " + responseArray[1] + " protocol");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible.  There should have been " + responseArray[1] + " scheduled recordings listed instead of just " + App.ViewModel.Upcoming.Count + ".", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());
        }
        private void ParseUpcoming31()
        {

            //Protocol version 31 and up - 42 fields

            App.ViewModel.Upcoming.Clear();

            string[] stringSeparators = new string[] { "[]:[]" };
            string[] responseArray;

            ProgramViewModel singleProgram = new ProgramViewModel();
            //int arrayIndex = 0;
            int programIndex = 0;
            int fieldIndex = 0;
            int i;

            int offset = 2; //upcoming is conflicting[]:[]qty programs[]:[]programs ...

            responseArray = fullProtocolResponse.Split(stringSeparators, StringSplitOptions.None);

            try
            {
                for (i = offset; i < responseArray.Length; i++)
                {
                    switch (fieldIndex)
                    {
                        case 0:
                            singleProgram = new ProgramViewModel();
                            singleProgram.title = responseArray[i];
                            break;
                        case 1:
                            singleProgram.subtitle = responseArray[i];
                            break;
                        case 2:
                            singleProgram.description = responseArray[i];
                            break;
                        case 3:
                            singleProgram.category = responseArray[i];
                            break;
                        case 4:
                            singleProgram.chanid = int.Parse(responseArray[i]);
                            singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + responseArray[i];
                            break;
                        case 5:
                            singleProgram.channum = responseArray[i];
                            break;
                        case 6:
                            singleProgram.callsign = responseArray[i];
                            break;
                        case 7:
                            singleProgram.channame = responseArray[i];
                            break;
                        case 8:
                            singleProgram.filename = responseArray[i];
                            break;
                        case 9:
                            //singleProgram.fs_high = Int64.Parse(responseArray[i]);
                            break;
                        case 10:
                            //singleProgram.fs_low = Int64.Parse(responseArray[i]);
                            break;

                        case 11:
                            singleProgram.starttimeint = Int64.Parse(responseArray[i]);
                            DateTime st = new DateTime(1970, 1, 1).AddSeconds(singleProgram.starttimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //st.Add(new TimeSpan(singleProgram.starttimeint * 10000000));
                            //st.AddSeconds(singleProgram.starttimeint);
                            singleProgram.starttime = st.ToString("s");
                            singleProgram.starttimespace = singleProgram.starttime.Replace("T", " ");
                            break;
                        case 12:
                            singleProgram.endtimeint = Int64.Parse(responseArray[i]);
                            DateTime et = new DateTime(1970, 1, 1).AddSeconds(singleProgram.endtimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //et.Add(new TimeSpan(singleProgram.endtimeint*10000000));
                            //et.AddSeconds(singleProgram.endtimeint);
                            singleProgram.endtime = et.ToString("s");
                            //asdf
                            break;
                        case 13:
                            //singleProgram.duplicate = responseArray[i];
                            break;
                        case 14:
                            //singleProgram.shareable = responseArray[i];
                            break;
                        case 15:
                            singleProgram.findid = responseArray[i];
                            break;
                        case 16:
                            singleProgram.hostname = responseArray[i];
                            break;
                        case 17:
                            singleProgram.sourceid = int.Parse(responseArray[i]);
                            break;
                        case 18:
                            singleProgram.cardid = int.Parse(responseArray[i]);
                            break;
                        case 19:
                            singleProgram.inputid = int.Parse(responseArray[i]);
                            break;

                        case 20:
                            singleProgram.recpriority = int.Parse(responseArray[i]);
                            break;
                        case 21:
                            singleProgram.recstatus = int.Parse(responseArray[i]);
                            singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //singleProgram.recstatustext = responseArray[i];
                            break;
                        case 22:
                            singleProgram.recordid = int.Parse(responseArray[i]);
                            break;
                        case 23:
                            singleProgram.rectype = int.Parse(responseArray[i]);
                            break;
                        case 24:
                            //singleProgram.dupin = responseArray[i];
                            break;
                        case 25:
                            //singleProgram.dupmethod = responseArray[i];
                            break;
                        case 26:
                            singleProgram.recstarttsint = Int64.Parse(responseArray[i]);
                            DateTime rt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recstarttsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //rt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //rt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recstartts = rt.ToString("s");
                            break;
                        case 27:
                            singleProgram.recendtsint = Int64.Parse(responseArray[i]);
                            DateTime tt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recendtsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //tt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //tt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recendts = tt.ToString("s");
                            //asdf
                            break;
                        case 28:
                            //singleProgram.repeat = int.Parse(responseArray[i]);
                            break;
                        case 29:
                            //singleProgram.programflags = int.Parse(responseArray[i]);
                            break;

                        case 30:
                            singleProgram.recgroup = responseArray[i];
                            break;
                        case 31:
                            //singleProgram.chancommfree = responseArray[i];
                            break;
                        case 32:
                            //singleProgram.outputfilters = responseArray[i];
                            break;
                        case 33:
                            singleProgram.seriesid = responseArray[i];
                            break;
                        case 34:
                            singleProgram.programid = responseArray[i];
                            break;
                        case 35:
                            singleProgram.lastmodified = responseArray[i];
                            break;
                        case 36:
                            //singleProgram.stars = responseArray[i];
                            break;
                        case 37:
                            singleProgram.airdate = responseArray[i];
                            break;
                        case 38:
                            //singleProgram.hasairdate = responseArray[i];
                            break;
                        case 39:
                            singleProgram.playgroup = responseArray[i];
                            break;

                        case 40:
                            singleProgram.recpriority2 = responseArray[i];
                            break;
                        case 41:
                            singleProgram.parentid = responseArray[i];

                            if (App.ViewModel.appSettings.ChannelIconsSetting)
                                singleProgram.showChanicon = System.Windows.Visibility.Visible;
                            else
                                singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                            App.ViewModel.Upcoming.Add(singleProgram);
                            programIndex++;
                            fieldIndex = -1;
                            break;
                    }

                    fieldIndex++;

                }

                //MessageBox.Show("Finished parsing programs: " + App.ViewModel.Upcoming.Count + " view model, " + programIndex + " program index, " + responseArray[1] + " protocol");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible.  There should have been " + responseArray[1] + " scheduled recordings listed instead of just " + App.ViewModel.Upcoming.Count + ".", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());
        }
        private void ParseUpcoming25()
        {


            //Protocol version 25 and up - 41 fields

            App.ViewModel.Upcoming.Clear();

            string[] stringSeparators = new string[] { "[]:[]" };
            string[] responseArray;

            ProgramViewModel singleProgram = new ProgramViewModel();
            //int arrayIndex = 0;
            int programIndex = 0;
            int fieldIndex = 0;
            int i;

            int offset = 2; //upcoming is conflicting[]:[]qty programs[]:[]programs ...

            responseArray = fullProtocolResponse.Split(stringSeparators, StringSplitOptions.None);

            try
            {
                for (i = offset; i < responseArray.Length; i++)
                {
                    switch (fieldIndex)
                    {
                        case 0:
                            singleProgram = new ProgramViewModel();
                            singleProgram.title = responseArray[i];
                            break;
                        case 1:
                            singleProgram.subtitle = responseArray[i];
                            break;
                        case 2:
                            singleProgram.description = responseArray[i];
                            break;
                        case 3:
                            singleProgram.category = responseArray[i];
                            break;
                        case 4:
                            singleProgram.chanid = int.Parse(responseArray[i]);
                            singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + responseArray[i];
                            break;
                        case 5:
                            singleProgram.channum = responseArray[i];
                            break;
                        case 6:
                            singleProgram.callsign = responseArray[i];
                            break;
                        case 7:
                            singleProgram.channame = responseArray[i];
                            break;
                        case 8:
                            singleProgram.filename = responseArray[i];
                            break;
                        case 9:
                            //singleProgram.fs_high = Int64.Parse(responseArray[i]);
                            break;
                        case 10:
                            //singleProgram.fs_low = Int64.Parse(responseArray[i]);
                            break;

                        case 11:
                            singleProgram.starttimeint = Int64.Parse(responseArray[i]);
                            DateTime st = new DateTime(1970, 1, 1).AddSeconds(singleProgram.starttimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //st.Add(new TimeSpan(singleProgram.starttimeint * 10000000));
                            //st.AddSeconds(singleProgram.starttimeint);
                            singleProgram.starttime = st.ToString("s");
                            singleProgram.starttimespace = singleProgram.starttime.Replace("T", " ");
                            break;
                        case 12:
                            singleProgram.endtimeint = Int64.Parse(responseArray[i]);
                            DateTime et = new DateTime(1970, 1, 1).AddSeconds(singleProgram.endtimeint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //et.Add(new TimeSpan(singleProgram.endtimeint*10000000));
                            //et.AddSeconds(singleProgram.endtimeint);
                            singleProgram.endtime = et.ToString("s");
                            //asdf
                            break;
                        case 13:
                            //singleProgram.duplicate = responseArray[i];
                            break;
                        case 14:
                            //singleProgram.shareable = responseArray[i];
                            break;
                        case 15:
                            singleProgram.findid = responseArray[i];
                            break;
                        case 16:
                            singleProgram.hostname = responseArray[i];
                            break;
                        case 17:
                            singleProgram.sourceid = int.Parse(responseArray[i]);
                            break;
                        case 18:
                            singleProgram.cardid = int.Parse(responseArray[i]);
                            break;
                        case 19:
                            singleProgram.inputid = int.Parse(responseArray[i]);
                            break;

                        case 20:
                            singleProgram.recpriority = int.Parse(responseArray[i]);
                            break;
                        case 21:
                            singleProgram.recstatus = int.Parse(responseArray[i]);
                            singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //singleProgram.recstatustext = responseArray[i];
                            break;
                        case 22:
                            singleProgram.recordid = int.Parse(responseArray[i]);
                            break;
                        case 23:
                            singleProgram.rectype = int.Parse(responseArray[i]);
                            break;
                        case 24:
                            //singleProgram.dupin = responseArray[i];
                            break;
                        case 25:
                            //singleProgram.dupmethod = responseArray[i];
                            break;
                        case 26:
                            singleProgram.recstarttsint = Int64.Parse(responseArray[i]);
                            DateTime rt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recstarttsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //rt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //rt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recstartts = rt.ToString("s");
                            break;
                        case 27:
                            singleProgram.recendtsint = Int64.Parse(responseArray[i]);
                            DateTime tt = new DateTime(1970, 1, 1).AddSeconds(singleProgram.recendtsint + (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds);
                            //tt.Add(new TimeSpan(singleProgram.recstarttsint * 10000000));
                            //tt.AddSeconds(singleProgram.recstarttsint);
                            singleProgram.recendts = tt.ToString("s");
                            //asdf
                            break;
                        case 28:
                            //singleProgram.repeat = int.Parse(responseArray[i]);
                            break;
                        case 29:
                            //singleProgram.programflags = int.Parse(responseArray[i]);
                            break;

                        case 30:
                            singleProgram.recgroup = responseArray[i];
                            break;
                        case 31:
                            //singleProgram.chancommfree = responseArray[i];
                            break;
                        case 32:
                            //singleProgram.outputfilters = responseArray[i];
                            break;
                        case 33:
                            singleProgram.seriesid = responseArray[i];
                            break;
                        case 34:
                            singleProgram.programid = responseArray[i];
                            break;
                        case 35:
                            singleProgram.lastmodified = responseArray[i];
                            break;
                        case 36:
                            //singleProgram.stars = responseArray[i];
                            break;
                        case 37:
                            singleProgram.airdate = responseArray[i];
                            break;
                        case 38:
                            //singleProgram.hasairdate = responseArray[i];
                            break;
                        case 39:
                            singleProgram.playgroup = responseArray[i];
                            break;

                        case 40:
                            singleProgram.recpriority2 = responseArray[i];

                            if (App.ViewModel.appSettings.ChannelIconsSetting)
                                singleProgram.showChanicon = System.Windows.Visibility.Visible;
                            else
                                singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                            App.ViewModel.Upcoming.Add(singleProgram);
                            programIndex++;
                            fieldIndex = -1;
                            break;
                    }

                    fieldIndex++;

                }

                //MessageBox.Show("Finished parsing programs: " + App.ViewModel.Upcoming.Count + " view model, " + programIndex + " program index, " + responseArray[1] + " protocol");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible.  There should have been " + responseArray[1] + " scheduled recordings listed instead of just " + App.ViewModel.Upcoming.Count + ".", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());
        }


        private void SortAndDisplay(string inConflicting)
        {

            AllUpcoming.Clear();
            ConflictingUpcoming.Clear();
            OverridesUpcoming.Clear();
            UpcomingUpcoming.Clear();

            //sorting

            var arr = App.ViewModel.Upcoming.OrderBy(x => x.starttime).ToArray();

            foreach (var item in arr)
            {

                AllUpcoming.Add(item);

                if (item.recstatus == 7)
                {
                    ConflictingUpcoming.Add(item);
                }

                if ((item.rectype == 7)||(item.rectype == 8))
                {
                    OverridesUpcoming.Add(item);
                }

                if ((item.recstatus == 7)||(item.recstatus == -1)||(item.recstatus == -2)||(item.recstatus == -10)||(item.recstatus == -9))
                {
                    UpcomingUpcoming.Add(item);
                }
            }


            var GroupedUpcomingUpcoming = from t in UpcomingUpcoming
                                          //group t by t.starttime.Substring(0, 10) into c
                                          group t by DateTime.Parse(t.starttime).ToString("dddd, MMMM dd") into c
                                          //orderby c.Key
                                          select new Group<ProgramViewModel>(c.Key, c);
            var GroupedAllUpcoming = from t in AllUpcoming
                                          //group t by t.starttime.Substring(0, 10) into c
                                          group t by DateTime.Parse(t.starttime).ToString("dddd, MMMM dd") into c
                                          //orderby c.Key
                                          select new Group<ProgramViewModel>(c.Key, c);
            var GroupedConflictingUpcoming = from t in ConflictingUpcoming
                                          //group t by t.starttime.Substring(0, 10) into c
                                          group t by DateTime.Parse(t.starttime).ToString("dddd, MMMM dd") into c
                                          //orderby c.Key
                                          select new Group<ProgramViewModel>(c.Key, c);
            var GroupedOverridesUpcoming = from t in OverridesUpcoming
                                          //group t by t.starttime.Substring(0, 10) into c
                                          group t by DateTime.Parse(t.starttime).ToString("dddd, MMMM dd") into c
                                          //orderby c.Key
                                          select new Group<ProgramViewModel>(c.Key, c);


            UpcomingUpcomingLL.ItemsSource = GroupedUpcomingUpcoming;
            AllUpcomingLL.ItemsSource = GroupedAllUpcoming;
            ConflictingUpcomingLL.ItemsSource = GroupedConflictingUpcoming;
            OverridesUpcomingLL.ItemsSource = GroupedOverridesUpcoming;


            AllTitle.Header = "all (" + AllUpcoming.Count + ")";
            ConflictingTitle.Header = "conflicting (" + ConflictingUpcoming.Count + ")";
            OverridesTitle.Header = "overrides (" + OverridesUpcoming.Count + ")";
            UpcomingTitle.Header = "upcoming (" + UpcomingUpcoming.Count + ")";

            performanceProgressBarCustomized.IsIndeterminate = false;

            if ((inConflicting == "1") || (ConflictingUpcoming.Count > 0)) BannerMessage("There are conflicting scheduled recordings");

            performanceProgressBarCustomized.IsIndeterminate = false;

            HasLoaded = true;
            
        }



        private void appbarRefresh_Click(object sender, EventArgs e)
        {
            GetUpcoming();
        }
            
        private void BannerMessage(string inMessage)
        {
            ToastPrompt toast = new ToastPrompt();

            toast.Title = inMessage;
            toast.TextOrientation = System.Windows.Controls.Orientation.Horizontal;

            toast.Show();
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
        

        private void UpcomingUpcomingLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (UpcomingUpcomingLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedUpcomingProgram = (ProgramViewModel)UpcomingUpcomingLL.SelectedItem;

            NavigationService.Navigate(new Uri("/UpcomingDetails.xaml", UriKind.Relative));

            UpcomingUpcomingLL.SelectedItem = null;
        }

        private void AllUpcomingLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (AllUpcomingLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedUpcomingProgram = (ProgramViewModel)AllUpcomingLL.SelectedItem;

            NavigationService.Navigate(new Uri("/UpcomingDetails.xaml", UriKind.Relative));

            AllUpcomingLL.SelectedItem = null;
        }

        private void ConflictingUpcomingLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ConflictingUpcomingLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedUpcomingProgram = (ProgramViewModel)ConflictingUpcomingLL.SelectedItem;

            NavigationService.Navigate(new Uri("/UpcomingDetails.xaml", UriKind.Relative));

            ConflictingUpcomingLL.SelectedItem = null;
        }

        private void OverridesUpcomingLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (OverridesUpcomingLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedUpcomingProgram = (ProgramViewModel)OverridesUpcomingLL.SelectedItem;

            NavigationService.Navigate(new Uri("/UpcomingDetails.xaml", UriKind.Relative));

            OverridesUpcomingLL.SelectedItem = null;
        }

        private void preferences_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Preferences.xaml", UriKind.Relative));
        }


    }

    public class ScriptProgram
    {
        public string title { get; set; }
        public string subtitle { get; set; }
        public string description { get; set; }
        public string category { get; set; }

        public string chanid { get; set; }
        public string channum { get; set; }
        public string callsign { get; set; }
        public string channame { get; set; }

        //public string filename { get; set; }
        //public string filesize { get; set; }
        public string starttime { get; set; }
        public string endtime { get; set; }

        //public string findid { get; set; }
        public string hostname { get; set; }
        public string sourceid { get; set; }
        public string cardid { get; set; }

        public string inputid { get; set; }
        //public string recpriority { get; set; }
        public string recstatus { get; set; }
        public string recordid { get; set; }

        public string rectype { get; set; }
        //public string dupin { get; set; }
        //public string dupmethod { get; set; }
        public string recstartts { get; set; }

        public string recendts { get; set; }
        //public string programflags { get; set; }
        public string recgroup { get; set; }
        //public string outputfilters { get; set; }

        public string seriesid { get; set; }
        public string programid { get; set; }
        //public string lastmodified { get; set; }
        //public string stars { get; set; }

        public string airdate { get; set; }
        public string playgroup { get; set; }
        public string recpriority2 { get; set; }
        public string parentid { get; set; }

        public string storagegroup { get; set; }
        public string audio_props { get; set; }
        public string video_props { get; set; }
        public string subtitle_type { get; set; }

        public string year { get; set; }
    }

}