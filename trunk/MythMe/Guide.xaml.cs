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

            this.Programs = new ObservableCollection<ProgramViewModel>();

            GuideNowListBox.ItemsSource = Programs;
        }

        private string getGuide25String = "http://{0}:{1}/Guide/GetProgramGuide?StartTime={2}&EndTime={3}&NumOfChannels={4}&StartChanId={5}&random={6}";
        private string getGuideString = "http://{0}:{1}/Myth/GetProgramGuide?StartTime={2}&EndTime={3}&NumOfChannels={4}&StartChanId={5}&random={6}";

        private ObservableCollection<ProgramViewModel> Programs;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string SelectedTime;
            if (NavigationContext.QueryString.TryGetValue("SelectedTime", out SelectedTime))
            {
                performanceProgressBarCustomized.IsIndeterminate = true;

                //MessageBox.Show("have time: "+SelectedTime);
                string newSelectedTime = SelectedTime.Substring(0, 17) + "01";
                this.Perform(() => GetGuide(newSelectedTime, newSelectedTime, "10000", ""), 50);
            }
            else if (NavigationContext.QueryString.TryGetValue("SelectedNow", out SelectedTime))
            {
                performanceProgressBarCustomized.IsIndeterminate = true;

                //MessageBox.Show("did not have time, do now");
                SelectedTime = DateTime.Now.ToString("s");
                string newSelectedTime = SelectedTime.Substring(0, 17) + "01";
                this.Perform(() => GetGuide(newSelectedTime, newSelectedTime, "10000", ""), 50);
            }
            else
            {
                performanceProgressBarCustomized.IsIndeterminate = false;
            }
        }

        private void GetGuide(string startTime, string endTime, string numChannels, string startChanid)
        {

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getGuide25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Guide25Callback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getGuideString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, startTime, endTime, numChannels, startChanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(GuideCallback), webRequest);
                //MessageBox.Show(String.Format(getGuideString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, startTime, endTime, numChannels, startChanid, App.ViewModel.randText()));
            }
        }

        private void Guide25Callback(IAsyncResult asynchronousResult)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("not yet supported"); });
        }

        private void GuideCallback(IAsyncResult asynchronousResult)
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

                        //singleProgram.programflags = (string)singleRecordedElement.Attribute("programFlags").Value;
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
                        //singleProgram.lastmodified = (string)singleRecordedElement.Attribute("lastModified").Value;

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


                        Deployment.Current.Dispatcher.BeginInvoke(() => { Programs.Add(singleProgram); });

                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    GuideNowListBox.ItemsSource = Programs;

                    performanceProgressBarCustomized.IsIndeterminate = false;
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



        private void Perform(Action myMethod, int delayInMilliseconds)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (s, e) => Thread.Sleep(delayInMilliseconds);

            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();

            worker.RunWorkerAsync();
        }

        private void GuideNowListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GuideNowListBox.SelectedItem == null)
                return;

            App.ViewModel.SelectedProgram = (ProgramViewModel)GuideNowListBox.SelectedItem;

            NavigationService.Navigate(new Uri("/GuideDetails.xaml", UriKind.Relative));

            GuideNowListBox.SelectedItem = null;
        }
    }
}