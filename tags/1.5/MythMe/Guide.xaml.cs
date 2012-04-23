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
using System.Xml.Linq;
using System.Security.Cryptography;

namespace MythMe
{
    public partial class Guide : PhoneApplicationPage
    {
        public Guide()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            this.NowPrograms = new ObservableCollection<ProgramViewModel>();
            this.TimePrograms = new ObservableCollection<ProgramViewModel>();
            this.ChannelPrograms = new ObservableCollection<ProgramViewModel>();

            externalTime = DateTime.Now.ToString("s");

            //NowGuideListBox.ItemsSource = NowPrograms;
            //TimeGuideListBox.ItemsSource = TimePrograms;
            //ChannelGuideListBox.ItemsSource = ChannelPrograms;
        }

        private string getGuide25String = "http://{0}:{1}/Guide/GetProgramGuide?StartTime={2}&EndTime={3}&NumChannels={4}&StartChanId={5}&random={6}";
        private string getGuideString = "http://{0}:{1}/Myth/GetProgramGuide?StartTime={2}&EndTime={3}&NumOfChannels={4}&StartChanId={5}&random={6}";

        private string externalTime;

        private ObservableCollection<ProgramViewModel> NowPrograms;
        private ObservableCollection<ProgramViewModel> TimePrograms;
        private ObservableCollection<ProgramViewModel> ChannelPrograms;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            string SelectedTime;

            if (App.ViewModel.appSettings.MasterBackendIpSetting == "")
            {
                MessageBox.Show("You need to enter a valid backend address in the preferences.");
                NavigationService.GoBack();
            }
            else if (NavigationContext.QueryString.TryGetValue("SelectedNow", out SelectedTime))
            {
                performanceProgressBarCustomized.IsIndeterminate = true;

                guidePivot.SelectedIndex = 0;

                NowPrograms.Clear();
                NowGuideLL.ItemsSource = null;

                //MessageBox.Show("did not have time, do now");
                SelectedTime = DateTime.Now.ToString("s");
                string newSelectedTime = SelectedTime.Substring(0, 17) + "01";
                this.Perform(() => GetNowGuide(newSelectedTime, newSelectedTime, "10000", ""), 50);
            }
            else if (NavigationContext.QueryString.TryGetValue("SelectedTime", out SelectedTime))
            {
                performanceProgressBarCustomized.IsIndeterminate = true;

                externalTime = SelectedTime;

                guidePivot.SelectedIndex = 1;

                TimePrograms.Clear();
                TimeGuideLL.ItemsSource = null;

                //MessageBox.Show("have time: "+SelectedTime);
                string newSelectedTime = SelectedTime.Substring(0, 17) + "01";
                this.Perform(() => GetTimeGuide(newSelectedTime, newSelectedTime, "10000", ""), 50);
            }
            else 
            {
                performanceProgressBarCustomized.IsIndeterminate = false;
            }

            NavigationContext.QueryString.Clear();
        }

        private void GetNowGuide(string startTime, string endTime, string numChannels, string startChanid)
        {

            NowPrograms.Clear();

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                //0.25 uses UTC time
                string newStartTime = DateTime.Parse(startTime).ToUniversalTime().ToString("s");
                string newEndTime = DateTime.Parse(endTime).ToUniversalTime().ToString("s");

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getGuide25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, newStartTime, newEndTime, numChannels, startChanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Guide25NowCallback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getGuideString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, startTime, endTime, numChannels, startChanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(GuideNowCallback), webRequest);
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.1.105/dropbox/GetProgramGuide.xml"));
                //webRequest.BeginGetResponse(new AsyncCallback(Guide25NowCallback), webRequest);
            }
        }

        private void GetTimeGuide(string startTime, string endTime, string numChannels, string startChanid)
        {

            TimePrograms.Clear();

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {

                //0.25 uses UTC time
                string newStartTime = DateTime.Parse(startTime).ToUniversalTime().ToString("s");
                string newEndTime = DateTime.Parse(endTime).ToUniversalTime().ToString("s");

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getGuide25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, newStartTime, newEndTime, numChannels, startChanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Guide25TimeCallback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getGuideString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, startTime, endTime, numChannels, startChanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(GuideTimeCallback), webRequest);
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.1.105/dropbox/GetProgramGuide.xml"));
                //webRequest.BeginGetResponse(new AsyncCallback(Guide25TimeCallback), webRequest);
            }
        }

        private void GetChannelGuide(string startTime, string endTime, string numChannels, string startChanid)
        {

            ChannelPrograms.Clear();

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                //0.25 uses UTC time
                string newStartTime = DateTime.Parse(startTime).ToUniversalTime().ToString("s");
                string newEndTime = DateTime.Parse(endTime).ToUniversalTime().ToString("s");

                
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getGuide25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, newStartTime, newEndTime, numChannels, startChanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Guide25ChannelCallback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getGuideString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, startTime, endTime, numChannels, startChanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(GuideChannelCallback), webRequest);
                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.1.105/dropbox/GetProgramGuide.xml"));
                //webRequest.BeginGetResponse(new AsyncCallback(Guide25ChannelCallback), webRequest);
            }
        }

        private void Guide25NowCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    App.ViewModel.Channels.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("ProgramGuide").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("ProgramGuide").Element("ProtoVer").Value);

                });

                foreach (XElement singleChannelElement in xdoc.Element("ProgramGuide").Element("Channels").Descendants("ChannelInfo"))
                {
                    ChannelViewModel singleChannel = new ChannelViewModel() { };

                    if (singleChannelElement.Element("ChannelName").FirstNode != null) singleChannel.channame = (string)singleChannelElement.Element("ChannelName").Value;
                    if (singleChannelElement.Element("ChanId").FirstNode != null) singleChannel.chanid = int.Parse(singleChannelElement.Element("ChanId").Value);
                    if (singleChannelElement.Element("ChanNum").FirstNode != null) singleChannel.channum = (string)singleChannelElement.Element("ChanNum").Value;
                    //singleChannel.channumint = int.Parse(singleChannelElement.Element("chanNum").Value);
                    if (singleChannelElement.Element("CallSign").FirstNode != null) singleChannel.callsign = (string)singleChannelElement.Element("CallSign").Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.ViewModel.Channels.Add(singleChannel);

                    });

                    foreach (XElement singleProgramElement in singleChannelElement.Element("Programs").Descendants("Program"))
                    {
                        ProgramViewModel singleProgram = new ProgramViewModel() { };

                        if (singleProgramElement.Element("Title").FirstNode != null) singleProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                        if (singleProgramElement.Element("SubTitle").FirstNode != null) singleProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                        //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                        if (singleProgramElement.Element("Category").FirstNode != null) singleProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                        //if (singleProgramElement.Element("FileSize").FirstNode != null) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                        //if (singleProgramElement.Element("SeriesId").FirstNode != null) singleProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                        //if (singleProgramElement.Element("HostName").FirstNode != null) singleProgram.hostname = (string)singleProgramElement.Element("HostName").FirstNode.ToString();
                        //singleProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                        //if (singleProgramElement.Element("ProgramId").FirstNode != null) singleProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                        //singleProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                        //singleProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                        if (singleProgramElement.Element("EndTime").FirstNode != null)
                        {
                            DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("EndTime").FirstNode.ToString());

                            singleProgram.endtime = newEndTime.ToLocalTime().ToString("s");
                            singleProgram.endtimespace = newEndTime.ToLocalTime().ToString("s").Replace("T"," ");
                        } 
                        //if (singleProgramElement.Element("Airdate").FirstNode != null) singleProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                        if (singleProgramElement.Element("StartTime").FirstNode != null)
                        {
                            DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("StartTime").FirstNode.ToString());

                            singleProgram.starttime = newStartTime.ToLocalTime().ToString("s");
                            singleProgram.starttimespace = newStartTime.ToLocalTime().ToString("s").Replace("T", " ");
                        }
                        //singleProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();


                        singleProgram.channame = singleChannel.channame;
                        singleProgram.chanid = singleChannel.chanid;
                        singleProgram.channum = singleChannel.channum;
                        //singleProgram.channumint = singleChannel.channumint;
                        singleProgram.callsign = singleChannel.callsign;
                        /*
                        */

                        
                        if (singleProgramElement.Descendants("Recording").Count() > 0)
                        {
                            if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                            if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                            //singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                            //if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                            //if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) singleProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                            //if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                        }
                        else
                        {
                            singleProgram.recstatus = -20;
                        }

                        singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);

                        //not sure how to get plain text as child of "Recording"
                        //singleProgram.description = (string)singleProgramElement.FirstNode.ToString();
                        
                        
                        singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Guide/GetChannelIcon?ChanId=" + singleProgram.chanid;

                        


                        if (App.ViewModel.appSettings.ChannelIconsSetting)
                            singleProgram.showChanicon = System.Windows.Visibility.Visible;
                        else
                            singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                        Deployment.Current.Dispatcher.BeginInvoke(() => { NowPrograms.Add(singleProgram); });

                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NowSortAndDisplay();

                    //NowGuideListBox.ItemsSource = NowPrograms;

                    //performanceProgressBarCustomized.IsIndeterminate = false;
                });

                try
                {

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });
            }
        }

        private void GuideNowCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                    App.ViewModel.Channels.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("GetProgramGuideResponse").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("GetProgramGuideResponse").Element("ProtoVer").Value);

                });

                foreach (XElement singleChannelElement in xdoc.Element("GetProgramGuideResponse").Element("ProgramGuide").Element("Channels").Descendants("Channel"))
                {
                    ChannelViewModel singleChannel = new ChannelViewModel() { };

                    singleChannel.channame = (string)singleChannelElement.Attribute("channelName").Value;
                    singleChannel.chanid = int.Parse(singleChannelElement.Attribute("chanId").Value);
                    singleChannel.channum = (string)singleChannelElement.Attribute("chanNum").Value;
                    //singleChannel.channumint = int.Parse(singleChannelElement.Attribute("chanNum").Value);
                    singleChannel.callsign = (string)singleChannelElement.Attribute("callSign").Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.ViewModel.Channels.Add(singleChannel);

                    });

                    foreach (XElement singleProgramElement in singleChannelElement.Descendants("Program"))
                    {
                        ProgramViewModel singleProgram = new ProgramViewModel() { };

                        singleProgram.title = (string)singleProgramElement.Attribute("title").Value;
                        singleProgram.subtitle = (string)singleProgramElement.Attribute("subTitle").Value;

                        //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").Value;
                        singleProgram.category = (string)singleProgramElement.Attribute("category").Value;
                        if (singleProgramElement.Attributes("fileSize").Count() > 0) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Attribute("fileSize").Value);
                        //singleProgram.seriesid = (string)singleProgramElement.Attribute("seriesId").Value;
                        //singleProgram.hostname = (string)singleProgramElement.Attribute("hostname").Value;
                        //singleProgram.cattype = (string)singleProgramElement.Attribute("catType").Value;
                        //singleProgram.programid = (string)singleProgramElement.Attribute("programId").Value;
                        //singleProgram.repeat = (string)singleProgramElement.Attribute("repeat").Value;
                        //singleProgram.stars = (string)singleProgramElement.Attribute("stars").Value;
                        singleProgram.endtime = (string)singleProgramElement.Attribute("endTime").Value;
                        singleProgram.endtimespace = (string)singleProgramElement.Attribute("endTime").Value.Replace("T", " ");
                        if (singleProgramElement.Attributes("airdate").Count() > 0) singleProgram.airdate = (string)singleProgramElement.Attribute("airdate").Value;
                        singleProgram.starttime = (string)singleProgramElement.Attribute("startTime").Value;
                        singleProgram.starttimespace = (string)singleProgramElement.Attribute("startTime").Value.Replace("T", " ");
                        //singleProgram.lastmodified = (string)singleProgramElement.Attribute("lastModified").Value;

                        singleProgram.channame = singleChannel.channame;
                        singleProgram.chanid = singleChannel.chanid;
                        singleProgram.channum = singleChannel.channum;
                        //singleProgram.channumint = singleChannel.channumint;
                        singleProgram.callsign = singleChannel.callsign;

                        singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + singleProgram.chanid;

                        if (singleProgramElement.Descendants("Recording").Count() > 0)
                        {
                            singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recPriority").Value);
                            singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recStatus").Value);
                            //singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedProgram.recstatus);
                            //singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Attribute("recGroup").Value;
                            singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Attribute("recStartTs").Value;
                            singleProgram.recendts = (string)singleProgramElement.Element("Recording").Attribute("recEndTs").Value;
                            //singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recordId").Value);

                        }
                        else
                        {
                            singleProgram.recstatus = -20;
                        }

                        singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);


                        if (App.ViewModel.appSettings.ChannelIconsSetting)
                            singleProgram.showChanicon = System.Windows.Visibility.Visible;
                        else
                            singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                        Deployment.Current.Dispatcher.BeginInvoke(() => { NowPrograms.Add(singleProgram); });

                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NowSortAndDisplay();
                    
                    //NowGuideListBox.ItemsSource = NowPrograms;

                    //performanceProgressBarCustomized.IsIndeterminate = false;
                });
            
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });
            }


        }

        private void Guide25TimeCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                    App.ViewModel.Channels.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("ProgramGuide").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("ProgramGuide").Element("ProtoVer").Value);

                });

                foreach (XElement singleChannelElement in xdoc.Element("ProgramGuide").Element("Channels").Descendants("ChannelInfo"))
                {
                    ChannelViewModel singleChannel = new ChannelViewModel() { };

                    if (singleChannelElement.Element("ChannelName").FirstNode != null) singleChannel.channame = (string)singleChannelElement.Element("ChannelName").Value;
                    if (singleChannelElement.Element("ChanId").FirstNode != null) singleChannel.chanid = int.Parse(singleChannelElement.Element("ChanId").Value);
                    if (singleChannelElement.Element("ChanNum").FirstNode != null) singleChannel.channum = (string)singleChannelElement.Element("ChanNum").Value;
                    //singleChannel.channumint = int.Parse(singleChannelElement.Element("chanNum").Value);
                    if (singleChannelElement.Element("CallSign").FirstNode != null) singleChannel.callsign = (string)singleChannelElement.Element("CallSign").Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.ViewModel.Channels.Add(singleChannel);

                    });

                    foreach (XElement singleProgramElement in singleChannelElement.Element("Programs").Descendants("Program"))
                    {
                        ProgramViewModel singleProgram = new ProgramViewModel() { };

                        if (singleProgramElement.Element("Title").FirstNode != null) singleProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                        if (singleProgramElement.Element("SubTitle").FirstNode != null) singleProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                        //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                        if (singleProgramElement.Element("Category").FirstNode != null) singleProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                        //if (singleProgramElement.Element("FileSize").FirstNode != null) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                        //if (singleProgramElement.Element("SeriesId").FirstNode != null) singleProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                        //if (singleProgramElement.Element("HostName").FirstNode != null) singleProgram.hostname = (string)singleProgramElement.Element("HostName").FirstNode.ToString();
                        //singleProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                        //if (singleProgramElement.Element("ProgramId").FirstNode != null) singleProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                        //singleProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                        //singleProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                        if (singleProgramElement.Element("EndTime").FirstNode != null)
                        {
                            DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("EndTime").FirstNode.ToString());

                            singleProgram.endtime = newEndTime.ToLocalTime().ToString("s");
                            singleProgram.endtimespace = newEndTime.ToLocalTime().ToString("s").Replace("T", " ");
                        }
                        //if (singleProgramElement.Element("Airdate").FirstNode != null) singleProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                        if (singleProgramElement.Element("StartTime").FirstNode != null)
                        {
                            DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("StartTime").FirstNode.ToString());

                            singleProgram.starttime = newStartTime.ToLocalTime().ToString("s");
                            singleProgram.starttimespace = newStartTime.ToLocalTime().ToString("s").Replace("T", " ");
                        }
                        //singleProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();


                        singleProgram.channame = singleChannel.channame;
                        singleProgram.chanid = singleChannel.chanid;
                        singleProgram.channum = singleChannel.channum;
                        //singleProgram.channumint = singleChannel.channumint;
                        singleProgram.callsign = singleChannel.callsign;
                        /*
                        */


                        if (singleProgramElement.Descendants("Recording").Count() > 0)
                        {
                            if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                            if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                            //singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                            //if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                            //if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) singleProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                            //if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                        }
                        else
                        {
                            singleProgram.recstatus = -20;
                        }

                        singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);

                        //not sure how to get plain text as child of "Recording"
                        //singleProgram.description = (string)singleProgramElement.FirstNode.ToString();


                        singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Guide/GetChannelIcon?ChanId=" + singleProgram.chanid;




                        if (App.ViewModel.appSettings.ChannelIconsSetting)
                            singleProgram.showChanicon = System.Windows.Visibility.Visible;
                        else
                            singleProgram.showChanicon = System.Windows.Visibility.Collapsed;


                        Deployment.Current.Dispatcher.BeginInvoke(() => { TimePrograms.Add(singleProgram); });

                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    TimeSortAndDisplay();

                    //NowGuideListBox.ItemsSource = NowPrograms;

                    //performanceProgressBarCustomized.IsIndeterminate = false;
                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });
            }
        }

        private void GuideTimeCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                    App.ViewModel.Channels.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("GetProgramGuideResponse").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("GetProgramGuideResponse").Element("ProtoVer").Value);

                });

                foreach (XElement singleChannelElement in xdoc.Element("GetProgramGuideResponse").Element("ProgramGuide").Element("Channels").Descendants("Channel"))
                {
                    ChannelViewModel singleChannel = new ChannelViewModel() { };

                    singleChannel.channame = (string)singleChannelElement.Attribute("channelName").Value;
                    singleChannel.chanid = int.Parse(singleChannelElement.Attribute("chanId").Value);
                    singleChannel.channum = (string)singleChannelElement.Attribute("chanNum").Value;
                    //singleChannel.channumint = int.Parse(singleChannelElement.Attribute("chanNum").Value);
                    singleChannel.callsign = (string)singleChannelElement.Attribute("callSign").Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.ViewModel.Channels.Add(singleChannel);

                    });

                    foreach (XElement singleProgramElement in singleChannelElement.Descendants("Program"))
                    {
                        ProgramViewModel singleProgram = new ProgramViewModel() { };

                        singleProgram.title = (string)singleProgramElement.Attribute("title").Value;
                        singleProgram.subtitle = (string)singleProgramElement.Attribute("subTitle").Value;

                        //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").Value;
                        singleProgram.category = (string)singleProgramElement.Attribute("category").Value;
                        if (singleProgramElement.Attributes("fileSize").Count() > 0) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Attribute("fileSize").Value);
                        //singleProgram.seriesid = (string)singleProgramElement.Attribute("seriesId").Value;
                        //singleProgram.hostname = (string)singleProgramElement.Attribute("hostname").Value;
                        //singleProgram.cattype = (string)singleProgramElement.Attribute("catType").Value;
                        //singleProgram.programid = (string)singleProgramElement.Attribute("programId").Value;
                        //singleProgram.repeat = (string)singleProgramElement.Attribute("repeat").Value;
                        //singleProgram.stars = (string)singleProgramElement.Attribute("stars").Value;
                        singleProgram.endtime = (string)singleProgramElement.Attribute("endTime").Value;
                        singleProgram.endtimespace = (string)singleProgramElement.Attribute("endTime").Value.Replace("T", " ");
                        if (singleProgramElement.Attributes("airdate").Count() > 0) singleProgram.airdate = (string)singleProgramElement.Attribute("airdate").Value;
                        singleProgram.starttime = (string)singleProgramElement.Attribute("startTime").Value;
                        singleProgram.starttimespace = (string)singleProgramElement.Attribute("startTime").Value.Replace("T", " ");
                        //singleProgram.lastmodified = (string)singleProgramElement.Attribute("lastModified").Value;

                        singleProgram.channame = singleChannel.channame;
                        singleProgram.chanid = singleChannel.chanid;
                        singleProgram.channum = singleChannel.channum;
                        //singleProgram.channumint = singleChannel.channumint;
                        singleProgram.callsign = singleChannel.callsign;

                        singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + singleProgram.chanid;

                        if (singleProgramElement.Descendants("Recording").Count() > 0)
                        {
                            singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recPriority").Value);
                            singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recStatus").Value);
                            //singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedProgram.recstatus);
                            //singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Attribute("recGroup").Value;
                            singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Attribute("recStartTs").Value;
                            singleProgram.recendts = (string)singleProgramElement.Element("Recording").Attribute("recEndTs").Value;
                            //singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recordId").Value);

                        }
                        else
                        {
                            singleProgram.recstatus = -20;
                        }

                        singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);

                        if (App.ViewModel.appSettings.ChannelIconsSetting)
                            singleProgram.showChanicon = System.Windows.Visibility.Visible;
                        else
                            singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                        Deployment.Current.Dispatcher.BeginInvoke(() => { TimePrograms.Add(singleProgram); });

                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    TimeSortAndDisplay();
                    
                    //TimeGuideListBox.ItemsSource = TimePrograms;

                    //performanceProgressBarCustomized.IsIndeterminate = false;
                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });
            }

        }

        private void Guide25ChannelCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                    App.ViewModel.Channels.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("ProgramGuide").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("ProgramGuide").Element("ProtoVer").Value);

                });

                foreach (XElement singleChannelElement in xdoc.Element("ProgramGuide").Element("Channels").Descendants("ChannelInfo"))
                {
                    ChannelViewModel singleChannel = new ChannelViewModel() { };

                    if (singleChannelElement.Element("ChannelName").FirstNode != null) singleChannel.channame = (string)singleChannelElement.Element("ChannelName").Value;
                    if (singleChannelElement.Element("ChanId").FirstNode != null) singleChannel.chanid = int.Parse(singleChannelElement.Element("ChanId").Value);
                    if (singleChannelElement.Element("ChanNum").FirstNode != null) singleChannel.channum = (string)singleChannelElement.Element("ChanNum").Value;
                    //singleChannel.channumint = int.Parse(singleChannelElement.Element("chanNum").Value);
                    if (singleChannelElement.Element("CallSign").FirstNode != null) singleChannel.callsign = (string)singleChannelElement.Element("CallSign").Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.ViewModel.Channels.Add(singleChannel);

                    });

                    foreach (XElement singleProgramElement in singleChannelElement.Element("Programs").Descendants("Program"))
                    {
                        ProgramViewModel singleProgram = new ProgramViewModel() { };

                        if (singleProgramElement.Element("Title").FirstNode != null) singleProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                        if (singleProgramElement.Element("SubTitle").FirstNode != null) singleProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                        //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                        if (singleProgramElement.Element("Category").FirstNode != null) singleProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                        //if (singleProgramElement.Element("FileSize").FirstNode != null) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                        //if (singleProgramElement.Element("SeriesId").FirstNode != null) singleProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                        //if (singleProgramElement.Element("HostName").FirstNode != null) singleProgram.hostname = (string)singleProgramElement.Element("HostName").FirstNode.ToString();
                        //singleProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                        //if (singleProgramElement.Element("ProgramId").FirstNode != null) singleProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                        //singleProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                        //singleProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                        if (singleProgramElement.Element("EndTime").FirstNode != null)
                        {
                            DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("EndTime").FirstNode.ToString());

                            singleProgram.endtime = newEndTime.ToLocalTime().ToString("s");
                            singleProgram.endtimespace = newEndTime.ToLocalTime().ToString("s").Replace("T", " ");
                        }
                        //if (singleProgramElement.Element("Airdate").FirstNode != null) singleProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                        if (singleProgramElement.Element("StartTime").FirstNode != null)
                        {
                            DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("StartTime").FirstNode.ToString());

                            singleProgram.starttime = newStartTime.ToLocalTime().ToString("s");
                            singleProgram.starttimespace = newStartTime.ToLocalTime().ToString("s").Replace("T", " ");
                        }
                        //singleProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();


                        singleProgram.channame = singleChannel.channame;
                        singleProgram.chanid = singleChannel.chanid;
                        singleProgram.channum = singleChannel.channum;
                        //singleProgram.channumint = singleChannel.channumint;
                        singleProgram.callsign = singleChannel.callsign;
                        /*
                        */


                        if (singleProgramElement.Descendants("Recording").Count() > 0)
                        {
                            if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                            if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                            //singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);
                            //if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                            //if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                            //if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) singleProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                            //if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                        }
                        else
                        {
                            singleProgram.recstatus = -20;
                        }

                        singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);

                        //not sure how to get plain text as child of "Recording"
                        //singleProgram.description = (string)singleProgramElement.FirstNode.ToString();


                        singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Guide/GetChannelIcon?ChanId=" + singleProgram.chanid;




                        if (App.ViewModel.appSettings.ChannelIconsSetting)
                            singleProgram.showChanicon = System.Windows.Visibility.Visible;
                        else
                            singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                        Deployment.Current.Dispatcher.BeginInvoke(() => { ChannelPrograms.Add(singleProgram); });

                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ChannelSortAndDisplay();

                    //NowGuideListBox.ItemsSource = NowPrograms;

                    //performanceProgressBarCustomized.IsIndeterminate = false;
                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });
            }
        }

        private void GuideChannelCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                    //App.ViewModel.Channels.Clear();

                    App.ViewModel.appSettings.MythBinarySetting = xdoc.Element("GetProgramGuideResponse").Element("Version").Value;
                    App.ViewModel.appSettings.ProtoVerSetting = int.Parse(xdoc.Element("GetProgramGuideResponse").Element("ProtoVer").Value);

                });

                foreach (XElement singleChannelElement in xdoc.Element("GetProgramGuideResponse").Element("ProgramGuide").Element("Channels").Descendants("Channel"))
                {
                    ChannelViewModel singleChannel = new ChannelViewModel() { };

                    singleChannel.channame = (string)singleChannelElement.Attribute("channelName").Value;
                    singleChannel.chanid = int.Parse(singleChannelElement.Attribute("chanId").Value);
                    singleChannel.channum = (string)singleChannelElement.Attribute("chanNum").Value;
                    //singleChannel.channumint = int.Parse(singleChannelElement.Attribute("chanNum").Value);
                    singleChannel.callsign = (string)singleChannelElement.Attribute("callSign").Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //App.ViewModel.Channels.Add(singleChannel);

                    });

                    foreach (XElement singleProgramElement in singleChannelElement.Descendants("Program"))
                    {
                        ProgramViewModel singleProgram = new ProgramViewModel() { };

                        singleProgram.title = (string)singleProgramElement.Attribute("title").Value;
                        singleProgram.subtitle = (string)singleProgramElement.Attribute("subTitle").Value;

                        //singleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").Value;
                        singleProgram.category = (string)singleProgramElement.Attribute("category").Value;
                        if (singleProgramElement.Attributes("fileSize").Count() > 0) singleProgram.filesize = Int64.Parse((string)singleProgramElement.Attribute("fileSize").Value);
                        //singleProgram.seriesid = (string)singleProgramElement.Attribute("seriesId").Value;
                        //singleProgram.hostname = (string)singleProgramElement.Attribute("hostname").Value;
                        //singleProgram.cattype = (string)singleProgramElement.Attribute("catType").Value;
                        //singleProgram.programid = (string)singleProgramElement.Attribute("programId").Value;
                        //singleProgram.repeat = (string)singleProgramElement.Attribute("repeat").Value;
                        //singleProgram.stars = (string)singleProgramElement.Attribute("stars").Value;
                        singleProgram.endtime = (string)singleProgramElement.Attribute("endTime").Value;
                        singleProgram.endtimespace = (string)singleProgramElement.Attribute("endTime").Value.Replace("T", " ");
                        if (singleProgramElement.Attributes("airdate").Count() > 0) singleProgram.airdate = (string)singleProgramElement.Attribute("airdate").Value;
                        singleProgram.starttime = (string)singleProgramElement.Attribute("startTime").Value;
                        singleProgram.starttimespace = (string)singleProgramElement.Attribute("startTime").Value.Replace("T", " ");
                        //singleProgram.lastmodified = (string)singleProgramElement.Attribute("lastModified").Value;

                        singleProgram.channame = singleChannel.channame;
                        singleProgram.chanid = singleChannel.chanid;
                        singleProgram.channum = singleChannel.channum;
                        //singleProgram.channumint = singleChannel.channumint;
                        singleProgram.callsign = singleChannel.callsign;

                        singleProgram.chanicon = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + singleProgram.chanid;

                        if (singleProgramElement.Descendants("Recording").Count() > 0)
                        {
                            singleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recPriority").Value);
                            singleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recStatus").Value);
                            //singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedProgram.recstatus);
                            //singleProgram.recgroup = (string)singleProgramElement.Element("Recording").Attribute("recGroup").Value;
                            singleProgram.recstartts = (string)singleProgramElement.Element("Recording").Attribute("recStartTs").Value;
                            singleProgram.recendts = (string)singleProgramElement.Element("Recording").Attribute("recEndTs").Value;
                            //singleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recordId").Value);

                        }
                        else
                        {
                            singleProgram.recstatus = -20;
                        }

                        singleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(singleProgram.recstatus);

                        if (App.ViewModel.appSettings.ChannelIconsSetting)
                            singleProgram.showChanicon = System.Windows.Visibility.Visible;
                        else
                            singleProgram.showChanicon = System.Windows.Visibility.Collapsed;

                        Deployment.Current.Dispatcher.BeginInvoke(() => { ChannelPrograms.Add(singleProgram); });

                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ChannelSortAndDisplay();

                    //TimeGuideListBox.ItemsSource = TimePrograms;

                    //performanceProgressBarCustomized.IsIndeterminate = false;
                });

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to get guide data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });
            }

        }

        private void SortAndDisplay()
        {
            switch (guidePivot.SelectedIndex)
            {
                case 0:
                    NowSortAndDisplay();
                    break;
                case 1:
                    TimeSortAndDisplay();
                    break;
                case 2:
                    ChannelSortAndDisplay();
                    break;
            }
        }

        private void NowSortAndDisplay()
        {

            var chans = App.ViewModel.Channels.OrderBy(x => x.channum).ToArray();

            guideChannelPicker.ItemsSource = chans;
            
            switch (App.ViewModel.appSettings.GuideSortSetting)
            {
                case "channel":
                    foreach (var item in NowPrograms)
                    {
                        item.guidesort = item.channum;
                        item.guidesortdisplay = item.channum + " - " + item.channame;
                    }
                    break;
                case "recstatus":
                    foreach (var item in NowPrograms)
                    {
                        item.guidesort = item.recstatustext;
                        item.guidesortdisplay = item.recstatustext;
                    }
                    break;
                case "title":
                    foreach (var item in NowPrograms)
                    {
                        item.guidesort = item.title;
                        item.guidesortdisplay = item.title;
                    }
                    break;
                default:
                    foreach (var item in NowPrograms)
                    {
                        item.guidesort = item.title;
                        item.guidesortdisplay = item.title;
                    }
                    break;
            }


            var arr = NowPrograms.OrderBy(x => x.guidesort).ToArray();

            switch (App.ViewModel.appSettings.GuideSortAscSetting)
            {
                case true:
                    //arr = NowPrograms.OrderBy(x => x.guidesort).ToArray();
                    break;
                case false:
                    arr = NowPrograms.OrderByDescending(x => x.guidesort).ToArray();
                    break;
            }

            /*
            NowPrograms.Clear();
            foreach (var item in arr)
            {
                NowPrograms.Add(item);
            }
            */


            var GroupedNowPrograms = from t in arr
                                         //group t by t.starttime.Substring(0, 10) into c
                                         group t by t.guidesortdisplay into c
                                         //orderby c.Key
                                         select new Group<ProgramViewModel>(c.Key, c);


            NowGuideLL.ItemsSource = GroupedNowPrograms;

            performanceProgressBarCustomized.IsIndeterminate = false;

        }

        private void TimeSortAndDisplay()
        {

            var chans = App.ViewModel.Channels.OrderBy(x => x.channum).ToArray();

            guideChannelPicker.ItemsSource = chans;

            switch (App.ViewModel.appSettings.GuideSortSetting)
            {
                case "channel":
                    foreach (var item in TimePrograms)
                    {
                        item.guidesort = item.channum;
                        item.guidesortdisplay = item.channum + " - " + item.channame;
                    }
                    break;
                case "recstatus":
                    foreach (var item in TimePrograms)
                    {
                        item.guidesort = item.recstatustext;
                        item.guidesortdisplay = item.recstatustext;
                    }
                    break;
                case "title":
                    foreach (var item in TimePrograms)
                    {
                        item.guidesort = item.title;
                        item.guidesortdisplay = item.title;
                    }
                    break;
                default:
                    foreach (var item in TimePrograms)
                    {
                        item.guidesort = item.title;
                        item.guidesortdisplay = item.title;
                    }
                    break;
            }


            var arr = TimePrograms.OrderBy(x => x.guidesort).ToArray();

            switch (App.ViewModel.appSettings.GuideSortAscSetting)
            {
                case true:
                    //arr = TimePrograms.OrderBy(x => x.guidesort).ToArray();
                    break;
                case false:
                    arr = TimePrograms.OrderByDescending(x => x.guidesort).ToArray();
                    break;
            }

            /*
            NowPrograms.Clear();
            foreach (var item in arr)
            {
                NowPrograms.Add(item);
            }
            */


            var GroupedTimePrograms = from t in arr
                                     //group t by t.starttime.Substring(0, 10) into c
                                     group t by t.guidesortdisplay into c
                                     //orderby c.Key
                                     select new Group<ProgramViewModel>(c.Key, c);


            TimeGuideLL.ItemsSource = GroupedTimePrograms;

            performanceProgressBarCustomized.IsIndeterminate = false;
        }


        private void ChannelSortAndDisplay()
        {

            switch (App.ViewModel.appSettings.GuideSortSetting)
            {
                case "channel":
                    foreach (var item in ChannelPrograms)
                    {
                        item.guidesort = item.starttime;
                        item.guidesortdisplay = item.channum + " - " + item.channame;
                    }
                    break;
                case "recstatus":
                    foreach (var item in ChannelPrograms)
                    {
                        item.guidesort = item.recstatustext;
                        item.guidesortdisplay = item.recstatustext;
                    }
                    break;
                case "title":
                    foreach (var item in ChannelPrograms)
                    {
                        item.guidesort = item.title;
                        item.guidesortdisplay = item.title;
                    }
                    break;
                default:
                    foreach (var item in ChannelPrograms)
                    {
                        item.guidesort = item.title;
                        item.guidesortdisplay = item.title;
                    }
                    break;
            }


            var arr = ChannelPrograms.OrderBy(x => x.guidesort).ToArray();

            switch (App.ViewModel.appSettings.GuideSortAscSetting)
            {
                case true:
                    //arr = ChannelPrograms.OrderBy(x => x.guidesort).ToArray();
                    break;
                case false:
                    arr = ChannelPrograms.OrderByDescending(x => x.guidesort).ToArray();
                    break;
            }


            var GroupedChannelPrograms = from t in arr
                                     //group t by t.starttime.Substring(0, 10) into c
                                     group t by t.guidesortdisplay into c
                                     //orderby c.Key
                                     select new Group<ProgramViewModel>(c.Key, c);


            ChannelGuideLL.ItemsSource = GroupedChannelPrograms;

            performanceProgressBarCustomized.IsIndeterminate = false;

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
            performanceProgressBarCustomized.IsIndeterminate = true;
            
            string SelectedTime;
            string newSelectedTime;

            switch (guidePivot.SelectedIndex)
            {
                case 0:
                    NowGuideLL.ItemsSource = null;
                    SelectedTime = DateTime.Now.ToString("s");
                    newSelectedTime = SelectedTime.Substring(0, 17) + "01";
                    this.Perform(() => GetNowGuide(newSelectedTime, newSelectedTime, "10000", ""), 50);
                    break;
                case 1:
                    TimeGuideLL.ItemsSource = null;
                    SelectedTime = externalTime;
                    newSelectedTime = SelectedTime.Substring(0, 17) + "01";
                    this.Perform(() => GetTimeGuide(newSelectedTime, newSelectedTime, "10000", ""), 50);
                    break;
                case 2:
                    ChannelGuideLL.ItemsSource = null;
                    ChannelViewModel chan = (ChannelViewModel)guideChannelPicker.SelectedItem;
                    DateTime date = (DateTime)guideChannelDatePicker.Value;

                    string beginTime = date.ToString("yyyy-MM-dd") + "T00:00:01";
                    string endTime = date.ToString("yyyy-MM-dd") + "T23:59:59";

                    this.Perform(() => GetChannelGuide(beginTime, endTime, "1", chan.chanid.ToString()), 50);
                    break;
            }
        }

        private void appbarSort_Click(object sender, EventArgs e)
        {
            SortPopup.IsOpen = true;
        }

        private void guidePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("changed to pivot index: " + guidePivot.SelectedIndex.ToString());

            //timeText.Text = externalTime.Replace("T", " ").Substring(0, 16);

            guideDatePicker.Value = DateTime.Parse(externalTime);
            guideTimePicker.Value = DateTime.Parse(externalTime);

            //guideChannelPicker.ItemsSource = App.ViewModel.Channels;
            guideChannelDatePicker.Value = DateTime.Parse(externalTime);
        }

        private void PopupChannelAsc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            App.ViewModel.appSettings.GuideSortSetting = "channel";
            App.ViewModel.appSettings.GuideSortAscSetting = true;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void PopupChannelDesc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            App.ViewModel.appSettings.GuideSortSetting = "channel";
            App.ViewModel.appSettings.GuideSortAscSetting = false;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void PopupRecstatusAsc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            App.ViewModel.appSettings.GuideSortSetting = "recstatus";
            App.ViewModel.appSettings.GuideSortAscSetting = false;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void PopupTitleAsc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.appSettings.GuideSortSetting = "title";
            App.ViewModel.appSettings.GuideSortAscSetting = true;

            SortPopup.IsOpen = false;

            SortAndDisplay();

        }

        private void PopupTitleDesc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            App.ViewModel.appSettings.GuideSortSetting = "title";
            App.ViewModel.appSettings.GuideSortAscSetting = false;

            SortPopup.IsOpen = false;

            SortAndDisplay();
        }

        private void NowGuideLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (NowGuideLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedGuideProgram = (ProgramViewModel)NowGuideLL.SelectedItem;

            NavigationService.Navigate(new Uri("/GuideDetails.xaml", UriKind.Relative));

            NowGuideLL.SelectedItem = null;
        }

        private void TimeGuideLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (TimeGuideLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedGuideProgram = (ProgramViewModel)TimeGuideLL.SelectedItem;

            NavigationService.Navigate(new Uri("/GuideDetails.xaml", UriKind.Relative));

            TimeGuideLL.SelectedItem = null;
        }

        private void ChannelGuideLL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ChannelGuideLL.SelectedItem == null)
                return;

            App.ViewModel.SelectedGuideProgram = (ProgramViewModel)ChannelGuideLL.SelectedItem;

            NavigationService.Navigate(new Uri("/GuideDetails.xaml", UriKind.Relative));

            ChannelGuideLL.SelectedItem = null;
        }

        private void guideTimePickerButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            performanceProgressBarCustomized.IsIndeterminate = true;
            
            DateTime date = (DateTime)guideDatePicker.Value;
            DateTime time = (DateTime)guideTimePicker.Value;

            externalTime = date.ToString("yyyy-MM-dd") + "T" + time.ToString("HH:mm")+":01";

            TimeGuideLL.ItemsSource = null;
            this.Perform(() => GetTimeGuide(externalTime, externalTime, "10000", ""), 50);

        }

        private void guideChannelButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            performanceProgressBarCustomized.IsIndeterminate = true;

            ChannelViewModel chan = (ChannelViewModel)guideChannelPicker.SelectedItem;
            DateTime date = (DateTime)guideChannelDatePicker.Value;

            string beginTime = date.ToString("yyyy-MM-dd") + "T00:00:01";
            string endTime = date.ToString("yyyy-MM-dd") + "T23:59:59";

            ChannelGuideLL.ItemsSource = null;
            this.Perform(() => GetChannelGuide(beginTime, endTime, "1", chan.chanid.ToString()), 50);
        }
    }
}