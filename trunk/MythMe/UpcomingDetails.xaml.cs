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

namespace MythMe
{
    public partial class UpcomingDetails : PhoneApplicationPage
    {
        public UpcomingDetails()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedProgram;
        }

        private string getDetails25String = "http://{0}:{1}/Guide/GetProgramDetails?StartTime={2}&ChanId={3}&random={2}";
        private string getDetailsString = "http://{0}:{1}/Myth/GetProgramDetails?StartTime={2}&ChanId={3}&random={4}";



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (App.ViewModel.Recorded.Count == 0) this.Perform(() => GetDetails(), 50);
        }

        private void GetDetails()
        {

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetails25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedProgram.starttime, App.ViewModel.SelectedProgram.chanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Details25Callback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetailsString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedProgram.starttime, App.ViewModel.SelectedProgram.chanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(DetailsCallback), webRequest);
            }
        }

        
        private void Details25Callback(IAsyncResult asynchronousResult)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("not yet supported"); });
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

                foreach (XElement singleProgramElement in xdoc.Element("GetProgramDetailsResponse").Element("ProgramDetails").Descendants("Program"))
                {
                    App.ViewModel.SelectedProgram.title = (string)singleProgramElement.Attribute("title").Value;
                    App.ViewModel.SelectedProgram.subtitle = (string)singleProgramElement.Attribute("subTitle").Value;

                    //App.ViewModel.SelectedProgram.programflags = (string)singleProgramElement.Attribute("programFlags").Value;
                    App.ViewModel.SelectedProgram.category = (string)singleProgramElement.Attribute("category").Value;
                    if (singleProgramElement.Attributes("fileSize").Count() > 0) App.ViewModel.SelectedProgram.filesize = Int64.Parse((string)singleProgramElement.Attribute("fileSize").Value);
                    App.ViewModel.SelectedProgram.seriesid = (string)singleProgramElement.Attribute("seriesId").Value;
                    App.ViewModel.SelectedProgram.hostname = (string)singleProgramElement.Attribute("hostname").Value;
                    //App.ViewModel.SelectedProgram.cattype = (string)singleProgramElement.Attribute("catType").Value;
                    App.ViewModel.SelectedProgram.programid = (string)singleProgramElement.Attribute("programId").Value;
                    //App.ViewModel.SelectedProgram.repeat = (string)singleProgramElement.Attribute("repeat").Value;
                    //App.ViewModel.SelectedProgram.stars = (string)singleProgramElement.Attribute("stars").Value;
                    App.ViewModel.SelectedProgram.endtime = (string)singleProgramElement.Attribute("endTime").Value;
                    App.ViewModel.SelectedProgram.endtimespace = (string)singleProgramElement.Attribute("endTime").Value.Replace("T", " ");
                    if (singleProgramElement.Attributes("airdate").Count() > 0) App.ViewModel.SelectedProgram.airdate = (string)singleProgramElement.Attribute("airdate").Value;
                    App.ViewModel.SelectedProgram.starttime = (string)singleProgramElement.Attribute("startTime").Value;
                    App.ViewModel.SelectedProgram.starttimespace = (string)singleProgramElement.Attribute("startTime").Value.Replace("T", " ");
                    //App.ViewModel.SelectedProgram.lastmodified = (string)singleProgramElement.Attribute("lastModified").Value;

                    if (singleProgramElement.Descendants("Recording").Count() > 0)
                    {
                        App.ViewModel.SelectedProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("inputId").Value);
                        App.ViewModel.SelectedProgram.channame = (string)singleProgramElement.Element("Channel").Attribute("channelName").Value;
                        App.ViewModel.SelectedProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("sourceId").Value);
                        App.ViewModel.SelectedProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("chanId").Value);
                        App.ViewModel.SelectedProgram.channum = (string)singleProgramElement.Element("Channel").Attribute("chanNum").Value;
                        App.ViewModel.SelectedProgram.callsign = (string)singleProgramElement.Element("Channel").Attribute("callSign").Value;
                    }


                    if (singleProgramElement.Descendants("Recording").Count() > 0)
                    {
                        App.ViewModel.SelectedProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recPriority").Value);
                        App.ViewModel.SelectedProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recStatus").Value);
                        //App.ViewModel.SelectedProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedProgram.recstatus);
                        App.ViewModel.SelectedProgram.recgroup = (string)singleProgramElement.Element("Recording").Attribute("recGroup").Value;
                        App.ViewModel.SelectedProgram.recstartts = (string)singleProgramElement.Element("Recording").Attribute("recStartTs").Value;
                        App.ViewModel.SelectedProgram.recendts = (string)singleProgramElement.Element("Recording").Attribute("recEndTs").Value;
                        App.ViewModel.SelectedProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recordId").Value);

                    }
                    else
                    {
                        App.ViewModel.SelectedProgram.recstatus = -20;
                    }

                    App.ViewModel.SelectedProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedProgram.recstatus);
                        
                }

                //Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("Done updating"); });
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error parsing details: "+ex.ToString()); });
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
            DateTime dateResult;
            DateTime.TryParse(App.ViewModel.SelectedProgram.starttime, out dateResult);

            //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
            TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
            //TimeSpan u = (dateResult - DateTime.Now);
            Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
            //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.MasterBackendIpSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedProgram.chanid + "/" + timestamp);
            webopen.Show();
        }

        private void guideButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.GuideTime = App.ViewModel.SelectedProgram.starttime;

            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedTime=" + App.ViewModel.SelectedProgram.starttime, UriKind.Relative));

        }
    }
}