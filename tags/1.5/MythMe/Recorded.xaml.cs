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

                    if(singleProgramElement.Element("Artwork").Element("ArtworkInfos").FirstNode != null)
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

                    singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + singleProgram.chanid;
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
                        App.ViewModel.Recorded.Add(singleProgram);

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


                foreach (XElement singleProgramElement in xdoc.Element("GetRecordedResponse").Element("Recorded").Element("Programs").Descendants("Program")) 
                {
                    ProgramViewModel singleProgram = new ProgramViewModel() { };

                    singleProgram.title = (string)singleProgramElement.Attribute("title").Value;
                    singleProgram.subtitle = (string)singleProgramElement.Attribute("subTitle").Value;

                    //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").Value;
                    singleProgram.category = (string)singleProgramElement.Attribute("category").Value;
                    if (singleProgramElement.Attributes("fileSize").Count() > 0) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Attribute("fileSize").Value);
                    singleProgram.seriesid = (string)singleProgramElement.Attribute("seriesId").Value;
                    singleProgram.hostname = (string)singleProgramElement.Attribute("hostname").Value;
                    //singleProgram.cattype = (string)singleProgramElement.Attribute("catType").Value;
                    singleProgram.programid = (string)singleProgramElement.Attribute("programId").Value;
                    //singleProgram.repeat = (string)singleProgramElement.Attribute("repeat").Value;
                    //singleProgram.stars = (string)singleProgramElement.Attribute("stars").Value;
                    singleProgram.endtime = (string)singleProgramElement.Attribute("endTime").Value;
                    singleProgram.endtimespace = (string)singleProgramElement.Attribute("endTime").Value.Replace("T", " ");
                    if (singleProgramElement.Attributes("airdate").Count() > 0) singleProgram.airdate = (string)singleProgramElement.Attribute("airdate").Value;
                    singleProgram.starttime = (string)singleProgramElement.Attribute("startTime").Value;
                    singleProgram.starttimespace = (string)singleProgramElement.Attribute("startTime").Value.Replace("T"," ");
                    //singleProgram.lastmodified = (string)singleProgramElement.Attribute("lastModified").Value;
                    
                    singleProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("inputId").Value);
                    singleProgram.channame = (string)singleProgramElement.Element("Channel").Attribute("channelName").Value;
                    singleProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("sourceId").Value);
                    singleProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("chanId").Value);
                    singleProgram.channum = (string)singleProgramElement.Element("Channel").Attribute("chanNum").Value;
                    singleProgram.callsign = (string)singleProgramElement.Element("Channel").Attribute("callSign").Value;
                    

                    singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recPriority").Value);
                    singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recStatus").Value);
                    singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                    singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Attribute("recGroup").Value;
                    singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Attribute("recStartTs").Value;
                    singleProgram.recendts = (string)singleProgramElement.Element("Recording").Attribute("recEndTs").Value;
                    singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recordId").Value);

                    singleProgram.description = (string)singleProgramElement.FirstNode.ToString();
                    if (singleProgram.description.Contains("<Channel")) singleProgram.description = "";
                    
                    
                    if (App.ViewModel.appSettings.ChannelIconsSetting)
                        singleProgram.showChanicon = System.Windows.Visibility.Visible;
                    else
                        singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                    singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + singleProgram.chanid;
                    singleProgram.screenshot = App.ViewModel.functions.CreateScreenshotUrl(singleProgram);

                    singleProgram.screenshot = App.ViewModel.functions.CreateScreenshotUrl(singleProgram);

                    if (singleProgram.recstatus == -2)
                    {
                        singleProgram.recordedfourthline = "Currently recording (" + singleProgram.channum + " - " + singleProgram.channame+")";
                    }
                    else
                    {
                        singleProgram.recordedfourthline = singleProgram.channum+" - "+singleProgram.channame;
                    }

                    if (singleProgram.subtitle == "") singleProgram.subtitle = ".";

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {   
                        //programlist.Add(singleProgram);
                        App.ViewModel.Recorded.Add(singleProgram);

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