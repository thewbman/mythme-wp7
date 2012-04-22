using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace MythMe
{
    public partial class Recorded : PhoneApplicationPage
    {
        public Recorded()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            //AllRecordedListBox.ItemsSource = AllRecorded;
            //DefaultRecordedListBox.ItemsSource = DefaultRecorded;
            //DeletedRecordedListBox.ItemsSource = DeletedRecorded;
            //LiveTVRecordedListBox.ItemsSource = LiveTVRecorded;
        }

        private string getRecorded25String = "http://{0}:{1}/Dvr/GetFilteredRecordedList?Count=1000&TitleRegEx=[A-Za-z0-9_]&random={2}";
        private string getRecordedString = "http://{0}:{1}/Myth/GetRecorded?random={2}";

        List<ProgramViewModel> AllRecorded = new List<ProgramViewModel>();
        List<ProgramViewModel> DefaultRecorded = new List<ProgramViewModel>();
        List<ProgramViewModel> DeletedRecorded = new List<ProgramViewModel>();
        List<ProgramViewModel> LiveTVRecorded = new List<ProgramViewModel>();

        private bool HasLoaded = false;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (App.ViewModel.appSettings.MasterBackendIpSetting == "")
            {
                MessageBox.Show("You need to enter a valid backend address in the preferences.");
                NavigationService.GoBack();
            }
            else if (App.ViewModel.Recorded.Count == 0) 
            {
                GetRecorded();
            }
            else if (!HasLoaded)
            {

                SortAndDisplay();

            }
            else
            {
                //do nothing
            }


        }

        private void GetRecorded()
        {

            performanceProgressBarCustomized.IsIndeterminate = true;

            AllTitle.Header = "all";
            DefaultTitle.Header = "default";
            DeletedTitle.Header = "deleted";
            LiveTVTitle.Header = "livetv";

            AllRecordedLL.ItemsSource = null;
            DefaultRecordedLL.ItemsSource = null;
            DeletedRecordedLL.ItemsSource = null;
            LiveTVRecordedLL.ItemsSource = null;

            AllRecorded.Clear();
            DefaultRecorded.Clear();
            DeletedRecorded.Clear();
            LiveTVRecorded.Clear();

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getRecorded25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting , App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Recorded25Callback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getRecordedString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(RecordedCallback), webRequest);
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.1.105/dropbox/GetRecorded.xml"));
                //webRequest.BeginGetResponse(new AsyncCallback(Recorded25Callback), webRequest);
            }
        }

        private void Recorded25Callback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get recorded data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                    App.ViewModel.Recorded.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("ProgramList").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("ProgramList").Element("ProtoVer").Value);

                });

                //ObservableCollection<ProgramViewModel> programlist = new ObservableCollection<ProgramViewModel>();
                //int recordedCount = 0;


                foreach (XElement singleUpcomingElement in xdoc.Element("ProgramList").Element("Programs").Descendants("Program"))
                {
                    ProgramViewModel singleUpcoming = new ProgramViewModel() { };

                    if (singleUpcomingElement.Element("Title").FirstNode != null) singleUpcoming.title = (string)singleUpcomingElement.Element("Title").FirstNode.ToString();
                    if (singleUpcomingElement.Element("SubTitle").FirstNode != null) singleUpcoming.subtitle = (string)singleUpcomingElement.Element("SubTitle").FirstNode.ToString();
                    
                    //singleUpcoming.programflags = (string)singleUpcomingElement.Attribute("programFlags").FirstNode.ToString();
                    if (singleUpcomingElement.Element("Category").FirstNode != null) singleUpcoming.category = (string)singleUpcomingElement.Element("Category").FirstNode.ToString();
                    if (singleUpcomingElement.Element("FileSize").FirstNode != null) singleUpcoming.filesize = Int64.Parse((string)singleUpcomingElement.Element("FileSize").FirstNode.ToString());
                    if (singleUpcomingElement.Element("SeriesId").FirstNode != null) singleUpcoming.seriesid = (string)singleUpcomingElement.Element("SeriesId").FirstNode.ToString();
                    if (singleUpcomingElement.Element("HostName").FirstNode != null) singleUpcoming.hostname = (string)singleUpcomingElement.Element("HostName").FirstNode.ToString();
                    //singleUpcoming.cattype = (string)singleUpcomingElement.Element("CatType").FirstNode.ToString();
                    if (singleUpcomingElement.Element("ProgramId").FirstNode != null) singleUpcoming.programid = (string)singleUpcomingElement.Element("ProgramId").FirstNode.ToString();
                    //singleUpcoming.repeat = (string)singleUpcomingElement.Element("Repeat").FirstNode.ToString();
                    //singleUpcoming.stars = (string)singleUpcomingElement.Element("Stars").FirstNode.ToString();
                    if (singleUpcomingElement.Element("EndTime").FirstNode != null)
                    {
                        DateTime newEndTime = DateTime.Parse((string)singleUpcomingElement.Element("EndTime").FirstNode.ToString());

                        singleUpcoming.endtime = newEndTime.ToLocalTime().ToString("s");
                        singleUpcoming.endtimespace = newEndTime.ToLocalTime().ToString("s").Replace("T", " ");
                    } 
                    if (singleUpcomingElement.Element("Airdate").FirstNode != null) singleUpcoming.airdate = (string)singleUpcomingElement.Element("Airdate").FirstNode.ToString();
                    if (singleUpcomingElement.Element("StartTime").FirstNode != null)
                    {
                        DateTime newStartTime = DateTime.Parse((string)singleUpcomingElement.Element("StartTime").FirstNode.ToString());

                        singleUpcoming.starttime = newStartTime.ToLocalTime().ToString("s");
                        singleUpcoming.starttimespace = newStartTime.ToLocalTime().ToString("s").Replace("T", " ");
                    } 
                    //singleUpcoming.lastmodified = (string)singleUpcomingElement.Element("lastModified").FirstNode.ToString();

                    if (singleUpcomingElement.Element("Channel").Element("InputId").FirstNode != null) singleUpcoming.inputid = int.Parse((string)singleUpcomingElement.Element("Channel").Element("InputId").Value);
                    if (singleUpcomingElement.Element("Channel").Element("ChannelName").FirstNode != null) singleUpcoming.channame = (string)singleUpcomingElement.Element("Channel").Element("ChannelName").Value;
                    if (singleUpcomingElement.Element("Channel").Element("SourceId").FirstNode != null) singleUpcoming.sourceid = int.Parse((string)singleUpcomingElement.Element("Channel").Element("SourceId").Value);
                    if (singleUpcomingElement.Element("Channel").Element("ChanId").FirstNode != null) singleUpcoming.chanid = int.Parse((string)singleUpcomingElement.Element("Channel").Element("ChanId").Value);
                    if (singleUpcomingElement.Element("Channel").Element("ChanNum").FirstNode != null) singleUpcoming.channum = (string)singleUpcomingElement.Element("Channel").Element("ChanNum").Value;
                    if (singleUpcomingElement.Element("Channel").Element("CallSign").FirstNode != null) singleUpcoming.callsign = (string)singleUpcomingElement.Element("Channel").Element("CallSign").Value;
                    /*
                    */

                    if (singleUpcomingElement.Element("Recording").Element("Priority").FirstNode != null) singleUpcoming.recpriority = int.Parse((string)singleUpcomingElement.Element("Recording").Element("Priority").Value);
                    if (singleUpcomingElement.Element("Recording").Element("Status").FirstNode != null) singleUpcoming.recstatus = int.Parse((string)singleUpcomingElement.Element("Recording").Element("Status").Value);
                    singleUpcoming.recstatustext = App.ViewModel.functions.RecStatusDecode(singleUpcoming.recstatus);
                    if (singleUpcomingElement.Element("Recording").Element("RecGroup").FirstNode != null) singleUpcoming.recgroup = (string)singleUpcomingElement.Element("Recording").Element("RecGroup").Value;
                    //if (singleUpcomingElement.Element("Recording").Element("StartTs").FirstNode != null) singleUpcoming.recstartts = (string)singleUpcomingElement.Element("Recording").Element("StartTs").Value;
                    //if (singleUpcomingElement.Element("Recording").Element("EndTs").FirstNode != null) singleUpcoming.recendts = (string)singleUpcomingElement.Element("Recording").Element("EndTs").Value;
                    if (singleUpcomingElement.Element("Recording").Element("RecordId").FirstNode != null) singleUpcoming.recordid = int.Parse((string)singleUpcomingElement.Element("Recording").Element("RecordId").Value);

                    if (singleUpcomingElement.Element("Recording").Element("StartTs").FirstNode != null)
                    {
                        DateTime newStartTime = DateTime.Parse((string)singleUpcomingElement.Element("Recording").Element("StartTs").FirstNode.ToString());

                        singleUpcoming.recstartts = newStartTime.ToLocalTime().ToString("s");
                    }
                    if (singleUpcomingElement.Element("Recording").Element("EndTs").FirstNode != null)
                    {
                        DateTime newEndTime = DateTime.Parse((string)singleUpcomingElement.Element("Recording").Element("EndTs").FirstNode.ToString());

                        singleUpcoming.recendts = newEndTime.ToLocalTime().ToString("s");
                    } 

                    if(singleUpcomingElement.Element("Artwork").Element("ArtworkInfos").FirstNode != null)
                    {
                        foreach (var singleArtworkInfoElement in singleUpcomingElement.Element("Artwork").Element("ArtworkInfos").Elements("ArtworkInfo"))
                        {
                            string arturlbase = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/";
                            string arturlend = "";
                            //string arturlend = "&Height=800&Width=1024";

                            switch (singleArtworkInfoElement.Element("Type").FirstNode.ToString())
                            {
                                case "coverart":
                                    singleUpcoming.coverart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                    break;
                                case "fanart":
                                    singleUpcoming.fanart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                    break;
                                case "banner":
                                    singleUpcoming.banner = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
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
                        singleUpcoming.showChanicon = System.Windows.Visibility.Visible;
                    else
                        singleUpcoming.showChanicon = System.Windows.Visibility.Collapsed;

                    singleUpcoming.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + singleUpcoming.chanid;
                    singleUpcoming.screenshot = App.ViewModel.functions.CreateScreenshotUrl(singleUpcoming);

                    if (singleUpcoming.recstatus == -2)
                    {
                        singleUpcoming.recordedfourthline = "Currently recording (" + singleUpcoming.channum + " - " + singleUpcoming.channame + ")";
                    }
                    else
                    {
                        singleUpcoming.recordedfourthline = singleUpcoming.channum + " - " + singleUpcoming.channame;
                    }

                    if (singleUpcoming.subtitle == "") singleUpcoming.subtitle = ".";

                    singleUpcoming.description = singleUpcomingElement.Element("Airdate").NextNode.ToString(); 
                    if (singleUpcomingElement.Element("Description").FirstNode != null) singleUpcoming.description = (string)singleUpcomingElement.Element("Description").FirstNode.ToString();
                    if (singleUpcoming.description.Contains("<Inet")) singleUpcoming.description = "";
                    if (singleUpcoming.description.Contains("<Desc")) singleUpcoming.description = "";

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //programlist.Add(singleUpcoming);
                        App.ViewModel.Recorded.Add(singleUpcoming);

                        /*
                        if (singleUpcoming.recgroup == "Default")
                        {
                            DefaultRecorded.Add(singleUpcoming);
                           // DefaultRecorded.OrderBy(p => p.title);
                        }
                        else if (singleUpcoming.recgroup == "Deleted")
                        {
                            DeletedRecorded.Add(singleUpcoming);
                            //DeletedRecorded.OrderBy(p => p.title);
                        }
                        else if (singleUpcoming.recgroup == "LiveTV")
                        {
                            LiveTVRecorded.Add(singleUpcoming);
                            //LiveTVRecorded.OrderBy(p => p.title);
                        }
                         */
                    });
                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    SortAndDisplay();
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

        private void RecordedCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get recorded data: "+ex.ToString(), "Error", MessageBoxButton.OK);
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

                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    App.ViewModel.Recorded.Clear(); 

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("GetRecordedResponse").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("GetRecordedResponse").Element("ProtoVer").Value);

                });

                //ObservableCollection<ProgramViewModel> programlist = new ObservableCollection<ProgramViewModel>();
                //int recordedCount = 0;


                foreach (XElement singleUpcomingElement in xdoc.Element("GetRecordedResponse").Element("Recorded").Element("Programs").Descendants("Program")) 
                {
                    ProgramViewModel singleUpcoming = new ProgramViewModel() { };

                    singleUpcoming.title = (string)singleUpcomingElement.Attribute("title").Value;
                    singleUpcoming.subtitle = (string)singleUpcomingElement.Attribute("subTitle").Value;

                    //singleUpcoming.programflags = (string)singleUpcomingElement.Attribute("programFlags").Value;
                    singleUpcoming.category = (string)singleUpcomingElement.Attribute("category").Value;
                    if (singleUpcomingElement.Attributes("fileSize").Count() > 0) singleUpcoming.filesize = Int64.Parse((string)singleUpcomingElement.Attribute("fileSize").Value);
                    singleUpcoming.seriesid = (string)singleUpcomingElement.Attribute("seriesId").Value;
                    singleUpcoming.hostname = (string)singleUpcomingElement.Attribute("hostname").Value;
                    //singleUpcoming.cattype = (string)singleUpcomingElement.Attribute("catType").Value;
                    singleUpcoming.programid = (string)singleUpcomingElement.Attribute("programId").Value;
                    //singleUpcoming.repeat = (string)singleUpcomingElement.Attribute("repeat").Value;
                    //singleUpcoming.stars = (string)singleUpcomingElement.Attribute("stars").Value;
                    singleUpcoming.endtime = (string)singleUpcomingElement.Attribute("endTime").Value;
                    singleUpcoming.endtimespace = (string)singleUpcomingElement.Attribute("endTime").Value.Replace("T", " ");
                    if (singleUpcomingElement.Attributes("airdate").Count() > 0) singleUpcoming.airdate = (string)singleUpcomingElement.Attribute("airdate").Value;
                    singleUpcoming.starttime = (string)singleUpcomingElement.Attribute("startTime").Value;
                    singleUpcoming.starttimespace = (string)singleUpcomingElement.Attribute("startTime").Value.Replace("T"," ");
                    //singleUpcoming.lastmodified = (string)singleUpcomingElement.Attribute("lastModified").Value;
                    
                    singleUpcoming.inputid = int.Parse((string)singleUpcomingElement.Element("Channel").Attribute("inputId").Value);
                    singleUpcoming.channame = (string)singleUpcomingElement.Element("Channel").Attribute("channelName").Value;
                    singleUpcoming.sourceid = int.Parse((string)singleUpcomingElement.Element("Channel").Attribute("sourceId").Value);
                    singleUpcoming.chanid = int.Parse((string)singleUpcomingElement.Element("Channel").Attribute("chanId").Value);
                    singleUpcoming.channum = (string)singleUpcomingElement.Element("Channel").Attribute("chanNum").Value;
                    singleUpcoming.callsign = (string)singleUpcomingElement.Element("Channel").Attribute("callSign").Value;
                    

                    singleUpcoming.recpriority = int.Parse((string)singleUpcomingElement.Element("Recording").Attribute("recPriority").Value);
                    singleUpcoming.recstatus = int.Parse((string)singleUpcomingElement.Element("Recording").Attribute("recStatus").Value);
                    singleUpcoming.recstatustext = App.ViewModel.functions.RecStatusDecode(singleUpcoming.recstatus);
                    singleUpcoming.recgroup = (string)singleUpcomingElement.Element("Recording").Attribute("recGroup").Value;
                    singleUpcoming.recstartts = (string)singleUpcomingElement.Element("Recording").Attribute("recStartTs").Value;
                    singleUpcoming.recendts = (string)singleUpcomingElement.Element("Recording").Attribute("recEndTs").Value;
                    singleUpcoming.recordid = int.Parse((string)singleUpcomingElement.Element("Recording").Attribute("recordId").Value);

                    singleUpcoming.description = (string)singleUpcomingElement.FirstNode.ToString();
                    if (singleUpcoming.description.Contains("<Channel")) singleUpcoming.description = "";
                    
                    
                    if (App.ViewModel.appSettings.ChannelIconsSetting)
                        singleUpcoming.showChanicon = System.Windows.Visibility.Visible;
                    else
                        singleUpcoming.showChanicon = System.Windows.Visibility.Collapsed;

                    singleUpcoming.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + singleUpcoming.chanid;
                    singleUpcoming.screenshot = App.ViewModel.functions.CreateScreenshotUrl(singleUpcoming);

                    singleUpcoming.screenshot = App.ViewModel.functions.CreateScreenshotUrl(singleUpcoming);

                    if (singleUpcoming.recstatus == -2)
                    {
                        singleUpcoming.recordedfourthline = "Currently recording (" + singleUpcoming.channum + " - " + singleUpcoming.channame+")";
                    }
                    else
                    {
                        singleUpcoming.recordedfourthline = singleUpcoming.channum+" - "+singleUpcoming.channame;
                    }

                    if (singleUpcoming.subtitle == "") singleUpcoming.subtitle = ".";

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {   
                        //programlist.Add(singleUpcoming);
                        App.ViewModel.Recorded.Add(singleUpcoming);

                        /*
                        if (singleUpcoming.recgroup == "Default")
                        {
                            DefaultRecorded.Add(singleUpcoming);
                           // DefaultRecorded.OrderBy(p => p.title);
                        }
                        else if (singleUpcoming.recgroup == "Deleted")
                        {
                            DeletedRecorded.Add(singleUpcoming);
                            //DeletedRecorded.OrderBy(p => p.title);
                        }
                        else if (singleUpcoming.recgroup == "LiveTV")
                        {
                            LiveTVRecorded.Add(singleUpcoming);
                            //LiveTVRecorded.OrderBy(p => p.title);
                        }
                         */
                    });
                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    SortAndDisplay();
                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to parse recorded data: "+ex.ToString(), "Error", MessageBoxButton.OK);
                    App.ViewModel.Connected = false;
                    NavigationService.GoBack();
                });

                return;
            }
        }

        private void SortAndDisplay()
        {

            AllRecorded.Clear();
            DefaultRecorded.Clear();
            DeletedRecorded.Clear();
            LiveTVRecorded.Clear();

            DateTime airdateDisplay;
            bool succesAirdate;

            switch (App.ViewModel.appSettings.RecSortSetting)
            {
                case "date":
                    foreach (var item in App.ViewModel.Recorded)
                    {
                        item.recsort = item.starttimespace;
                        item.recsortdisplay = DateTime.Parse(item.starttime).ToString("dddd, MMMM dd, yyyy");
                    }
                    break;
                case "airdate":
                    foreach (var item in App.ViewModel.Recorded)
                    {
                        item.recsort = item.airdate;
                        succesAirdate = DateTime.TryParse(item.airdate, out airdateDisplay);

                        if(succesAirdate)
                            item.recsortdisplay = airdateDisplay.ToString("MMMM dd, yyyy");
                        else
                            item.recsortdisplay = DateTime.Parse("1900-01-01").ToString("MMMM dd, yyyy");
                    }
                    break;
                case "title":
                    foreach (var item in App.ViewModel.Recorded)
                    {
                        item.recsort = item.title;
                        item.recsortdisplay = item.title;
                    }
                    break;
                default:
                    foreach (var item in App.ViewModel.Recorded)
                    {
                        item.recsort = item.title;
                        item.recsortdisplay = item.title;
                    }
                    break;
            }


            var arr = App.ViewModel.Recorded.OrderBy(x => x.recsort).ToArray();

            switch (App.ViewModel.appSettings.RecSortAscSetting)
            {
                case true:
                    //arr = App.ViewModel.Recorded.OrderBy(x => x.recsort).ToArray();
                    break;
                case false:
                    arr = App.ViewModel.Recorded.OrderByDescending(x => x.recsort).ToArray();
                    break;
            }

            App.ViewModel.Recorded.Clear();
            foreach (var item in arr)
            {
                App.ViewModel.Recorded.Add(item);

                AllRecorded.Add(item);

                if (item.recgroup == "Default")
                {
                    DefaultRecorded.Add(item);
                }
                else if (item.recgroup == "Deleted")
                {
                    DeletedRecorded.Add(item);
                }
                else if (item.recgroup == "LiveTV")
                {
                    LiveTVRecorded.Add(item);
                }
            }


            var GroupedDefaultRecorded = from t in DefaultRecorded
                                          //group t by t.starttime.Substring(0, 10) into c
                                          group t by t.recsortdisplay into c
                                          //orderby c.Key
                                         select new Group<ProgramViewModel>(c.Key, c);
            var GroupedDeletedRecorded = from t in DeletedRecorded
                                         //group t by t.starttime.Substring(0, 10) into c
                                         group t by t.recsortdisplay into c
                                         //orderby c.Key
                                         select new Group<ProgramViewModel>(c.Key, c);
            var GroupedLiveTVRecorded = from t in LiveTVRecorded
                                         //group t by t.starttime.Substring(0, 10) into c
                                         group t by t.recsortdisplay into c
                                         //orderby c.Key
                                         select new Group<ProgramViewModel>(c.Key, c);
            var GroupedAllRecorded = from t in AllRecorded
                                         //group t by t.starttime.Substring(0, 10) into c
                                         group t by t.recsortdisplay into c
                                         //orderby c.Key
                                         select new Group<ProgramViewModel>(c.Key, c);


            DefaultRecordedLL.ItemsSource = GroupedDefaultRecorded;
            DeletedRecordedLL.ItemsSource = GroupedDeletedRecorded;
            LiveTVRecordedLL.ItemsSource = GroupedLiveTVRecorded;
            AllRecordedLL.ItemsSource = GroupedAllRecorded;


            AllTitle.Header = "all (" + AllRecorded.Count + ")";
            DefaultTitle.Header = "default (" + DefaultRecorded.Count + ")";
            DeletedTitle.Header = "deleted (" + DeletedRecorded.Count + ")";
            LiveTVTitle.Header = "livetv (" + LiveTVRecorded.Count + ")";

            performanceProgressBarCustomized.IsIndeterminate = false;

            HasLoaded = true;

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
            GetRecorded();
        }

        private void appbarSort_Click(object sender, EventArgs e)
        {
            SortPopup.IsOpen = true;
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


        private void DefaultRecordedLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (DefaultRecordedLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedRecordedProgram = (ProgramViewModel)DefaultRecordedLL.SelectedItem;

            NavigationService.Navigate(new Uri("/RecordedDetails.xaml", UriKind.Relative));

            DefaultRecordedLL.SelectedItem = null;
        }

        private void DeletedRecordedLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (DeletedRecordedLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedRecordedProgram = (ProgramViewModel)DeletedRecordedLL.SelectedItem;

            NavigationService.Navigate(new Uri("/RecordedDetails.xaml", UriKind.Relative));

            DeletedRecordedLL.SelectedItem = null;
        }

        private void LiveTVRecordedLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (LiveTVRecordedLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedRecordedProgram = (ProgramViewModel)LiveTVRecordedLL.SelectedItem;

            NavigationService.Navigate(new Uri("/RecordedDetails.xaml", UriKind.Relative));

            LiveTVRecordedLL.SelectedItem = null;
        }

        private void AllRecordedLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (AllRecordedLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedRecordedProgram = (ProgramViewModel)AllRecordedLL.SelectedItem;

            NavigationService.Navigate(new Uri("/RecordedDetails.xaml", UriKind.Relative));

            AllRecordedLL.SelectedItem = null;
        }

        private void PopupDateAsc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.appSettings.RecSortSetting = "date";
            App.ViewModel.appSettings.RecSortAscSetting = true;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void PopupDateDesc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.appSettings.RecSortSetting = "date";
            App.ViewModel.appSettings.RecSortAscSetting = false;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void PopupAirateAsc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.appSettings.RecSortSetting = "airdate";
            App.ViewModel.appSettings.RecSortAscSetting = true;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void PopupAirdateDesc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.appSettings.RecSortSetting = "airdate";
            App.ViewModel.appSettings.RecSortAscSetting = false;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void PopupTitleAsc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            App.ViewModel.appSettings.RecSortSetting = "title";
            App.ViewModel.appSettings.RecSortAscSetting = true;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void PopupTitleDesc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            App.ViewModel.appSettings.RecSortSetting = "title";
            App.ViewModel.appSettings.RecSortAscSetting = false;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

    }
}