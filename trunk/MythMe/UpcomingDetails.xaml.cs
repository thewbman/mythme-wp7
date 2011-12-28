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
using System.Runtime.Serialization.Json;

namespace MythMe
{
    public partial class UpcomingDetails : PhoneApplicationPage
    {
        public UpcomingDetails()
        {
            InitializeComponent();

            People = new List<PeopleViewModel>();
            encoder = new UTF8Encoding();

            DataContext = App.ViewModel.SelectedUpcomingProgram;
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

            }
            else
            {
                peoplePivot.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.Perform(() => GetDetails(), 50);
        }

        private void GetDetails()
        {

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetails25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedUpcomingProgram.starttime, App.ViewModel.SelectedUpcomingProgram.chanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Details25Callback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetailsString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedUpcomingProgram.starttime, App.ViewModel.SelectedUpcomingProgram.chanid, App.ViewModel.randText())));
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

                if (singleProgramElement.Element("Title").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                if (singleProgramElement.Element("SubTitle").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                //App.ViewModel.SelectedUpcomingProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                if (singleProgramElement.Element("Category").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                if (singleProgramElement.Element("FileSize").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                if (singleProgramElement.Element("SeriesId").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                if (singleProgramElement.Element("Hostname").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.hostname = (string)singleProgramElement.Element("Hostname").FirstNode.ToString();
                //App.ViewModel.SelectedUpcomingProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                if (singleProgramElement.Element("ProgramId").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                //App.ViewModel.SelectedUpcomingProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                //App.ViewModel.SelectedUpcomingProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                if (singleProgramElement.Element("EndTime").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.endtime = (string)singleProgramElement.Element("EndTime").FirstNode.ToString();
                if (singleProgramElement.Element("EndTime").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.endtimespace = (string)singleProgramElement.Element("EndTime").FirstNode.ToString().Replace("T", " ");
                if (singleProgramElement.Element("Airdate").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                if (singleProgramElement.Element("StartTime").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.starttime = (string)singleProgramElement.Element("StartTime").FirstNode.ToString();
                if (singleProgramElement.Element("StartTime").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.starttimespace = (string)singleProgramElement.Element("StartTime").FirstNode.ToString().Replace("T", " ");
                //App.ViewModel.SelectedUpcomingProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();


                App.ViewModel.SelectedUpcomingProgram.description = singleProgramElement.Element("Airdate").NextNode.ToString();
                if (App.ViewModel.SelectedUpcomingProgram.description.Contains("<Inet")) App.ViewModel.SelectedUpcomingProgram.description = "";


                if (App.ViewModel.SelectedUpcomingProgram.subtitle == "") App.ViewModel.SelectedUpcomingProgram.subtitle = ".";

                if (singleProgramElement.Descendants("Channel").Count() > 0)
                {

                    if (singleProgramElement.Element("Channel").Element("InputId").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Element("InputId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChannelName").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.channame = (string)singleProgramElement.Element("Channel").Element("ChannelName").Value;
                    if (singleProgramElement.Element("Channel").Element("SourceId").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Element("SourceId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanId").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Element("ChanId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanNum").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.channum = (string)singleProgramElement.Element("Channel").Element("ChanNum").Value;
                    if (singleProgramElement.Element("Channel").Element("CallSign").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.callsign = (string)singleProgramElement.Element("Channel").Element("CallSign").Value;

                }


                if (singleProgramElement.Descendants("Recording").Count() > 0)
                {
                    if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                    if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                    //App.ViewModel.SelectedUpcomingProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedUpcomingProgram.recstatus);
                    if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                    if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                    if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                    if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) App.ViewModel.SelectedUpcomingProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                }
                else
                {
                    App.ViewModel.SelectedUpcomingProgram.recstatus = -20;
                }

                App.ViewModel.SelectedUpcomingProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedUpcomingProgram.recstatus);


                if (App.ViewModel.SelectedUpcomingProgram.recstatus == -2)
                {
                    App.ViewModel.SelectedUpcomingProgram.recordedfourthline = "Currently recording (" + App.ViewModel.SelectedUpcomingProgram.channum + " - " + App.ViewModel.SelectedUpcomingProgram.channame + ")";
                }
                else
                {
                    App.ViewModel.SelectedUpcomingProgram.recordedfourthline = App.ViewModel.SelectedUpcomingProgram.channum + " - " + App.ViewModel.SelectedUpcomingProgram.channame;
                }




                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Done updating");

                    DataContext = App.ViewModel.SelectedUpcomingProgram;
                    topPanorama.DataContext = App.ViewModel.SelectedUpcomingProgram;

                    recstatustext.Text = App.ViewModel.SelectedUpcomingProgram.recstatustext;
                    title.Text = App.ViewModel.SelectedUpcomingProgram.title;
                    subtitle.Text = App.ViewModel.SelectedUpcomingProgram.subtitle;
                    category.Text = App.ViewModel.SelectedUpcomingProgram.category;
                    description.Text = App.ViewModel.SelectedUpcomingProgram.description;

                    starttime.Text = App.ViewModel.SelectedUpcomingProgram.starttime;
                    endtime.Text = App.ViewModel.SelectedUpcomingProgram.endtime;
                    seriesid.Text = App.ViewModel.SelectedUpcomingProgram.seriesid;
                    programid.Text = App.ViewModel.SelectedUpcomingProgram.programid;
                    airdate.Text = App.ViewModel.SelectedUpcomingProgram.airdate;

                    channum.Text = App.ViewModel.SelectedUpcomingProgram.channum;
                    channame.Text = App.ViewModel.SelectedUpcomingProgram.channame;
                    chanid.Text = App.ViewModel.SelectedUpcomingProgram.chanid.ToString();

                    recstatustext2.Text = App.ViewModel.SelectedUpcomingProgram.recstatustext;
                    recstartts.Text = App.ViewModel.SelectedUpcomingProgram.recstartts;
                    recendts.Text = App.ViewModel.SelectedUpcomingProgram.recendts;
                    //hostname.Text = App.ViewModel.SelectedUpcomingProgram.hostname;
                    recgroup.Text = App.ViewModel.SelectedUpcomingProgram.recgroup;

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
                    App.ViewModel.SelectedUpcomingProgram.title = (string)singleProgramElement.Attribute("title").Value;
                    App.ViewModel.SelectedUpcomingProgram.subtitle = (string)singleProgramElement.Attribute("subTitle").Value;

                    //App.ViewModel.SelectedUpcomingProgram.programflags = (string)singleProgramElement.Attribute("programFlags").Value;
                    App.ViewModel.SelectedUpcomingProgram.category = (string)singleProgramElement.Attribute("category").Value;
                    if (singleProgramElement.Attributes("fileSize").Count() > 0) App.ViewModel.SelectedUpcomingProgram.filesize = Int64.Parse((string)singleProgramElement.Attribute("fileSize").Value);
                    App.ViewModel.SelectedUpcomingProgram.seriesid = (string)singleProgramElement.Attribute("seriesId").Value;
                    App.ViewModel.SelectedUpcomingProgram.hostname = (string)singleProgramElement.Attribute("hostname").Value;
                    //App.ViewModel.SelectedUpcomingProgram.cattype = (string)singleProgramElement.Attribute("catType").Value;
                    App.ViewModel.SelectedUpcomingProgram.programid = (string)singleProgramElement.Attribute("programId").Value;
                    //App.ViewModel.SelectedUpcomingProgram.repeat = (string)singleProgramElement.Attribute("repeat").Value;
                    //App.ViewModel.SelectedUpcomingProgram.stars = (string)singleProgramElement.Attribute("stars").Value;
                    App.ViewModel.SelectedUpcomingProgram.endtime = (string)singleProgramElement.Attribute("endTime").Value;
                    App.ViewModel.SelectedUpcomingProgram.endtimespace = (string)singleProgramElement.Attribute("endTime").Value.Replace("T", " ");
                    if (singleProgramElement.Attributes("airdate").Count() > 0) App.ViewModel.SelectedUpcomingProgram.airdate = (string)singleProgramElement.Attribute("airdate").Value;
                    App.ViewModel.SelectedUpcomingProgram.starttime = (string)singleProgramElement.Attribute("startTime").Value;
                    App.ViewModel.SelectedUpcomingProgram.starttimespace = (string)singleProgramElement.Attribute("startTime").Value.Replace("T", " ");
                    //App.ViewModel.SelectedUpcomingProgram.lastmodified = (string)singleProgramElement.Attribute("lastModified").Value;

                    if (singleProgramElement.Descendants("Recording").Count() > 0)
                    {
                        App.ViewModel.SelectedUpcomingProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("inputId").Value);
                        App.ViewModel.SelectedUpcomingProgram.channame = (string)singleProgramElement.Element("Channel").Attribute("channelName").Value;
                        App.ViewModel.SelectedUpcomingProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("sourceId").Value);
                        App.ViewModel.SelectedUpcomingProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("chanId").Value);
                        App.ViewModel.SelectedUpcomingProgram.channum = (string)singleProgramElement.Element("Channel").Attribute("chanNum").Value;
                        App.ViewModel.SelectedUpcomingProgram.callsign = (string)singleProgramElement.Element("Channel").Attribute("callSign").Value;
                    }


                    if (singleProgramElement.Descendants("Recording").Count() > 0)
                    {
                        App.ViewModel.SelectedUpcomingProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recPriority").Value);
                        App.ViewModel.SelectedUpcomingProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recStatus").Value);
                        //App.ViewModel.SelectedUpcomingProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedUpcomingProgram.recstatus);
                        App.ViewModel.SelectedUpcomingProgram.recgroup = (string)singleProgramElement.Element("Recording").Attribute("recGroup").Value;
                        App.ViewModel.SelectedUpcomingProgram.recstartts = (string)singleProgramElement.Element("Recording").Attribute("recStartTs").Value;
                        App.ViewModel.SelectedUpcomingProgram.recendts = (string)singleProgramElement.Element("Recording").Attribute("recEndTs").Value;
                        App.ViewModel.SelectedUpcomingProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recordId").Value);

                    }
                    else
                    {
                        App.ViewModel.SelectedUpcomingProgram.recstatus = -20;
                    }

                    App.ViewModel.SelectedUpcomingProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedUpcomingProgram.recstatus);
                        
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Done updating");

                    DataContext = App.ViewModel.SelectedUpcomingProgram;
                    topPanorama.DataContext = App.ViewModel.SelectedUpcomingProgram;

                    recstatustext.Text = App.ViewModel.SelectedUpcomingProgram.recstatustext;
                    title.Text = App.ViewModel.SelectedUpcomingProgram.title;
                    subtitle.Text = App.ViewModel.SelectedUpcomingProgram.subtitle;
                    category.Text = App.ViewModel.SelectedUpcomingProgram.category;
                    description.Text = App.ViewModel.SelectedUpcomingProgram.description;

                    starttime.Text = App.ViewModel.SelectedUpcomingProgram.starttime;
                    endtime.Text = App.ViewModel.SelectedUpcomingProgram.endtime;
                    seriesid.Text = App.ViewModel.SelectedUpcomingProgram.seriesid;
                    programid.Text = App.ViewModel.SelectedUpcomingProgram.programid;
                    airdate.Text = App.ViewModel.SelectedUpcomingProgram.airdate;

                    channum.Text = App.ViewModel.SelectedUpcomingProgram.channum;
                    channame.Text = App.ViewModel.SelectedUpcomingProgram.channame;
                    chanid.Text = App.ViewModel.SelectedUpcomingProgram.chanid.ToString();

                    recstatustext2.Text = App.ViewModel.SelectedUpcomingProgram.recstatustext;
                    recstartts.Text = App.ViewModel.SelectedUpcomingProgram.recstartts;
                    recendts.Text = App.ViewModel.SelectedUpcomingProgram.recendts;
                    //hostname.Text = App.ViewModel.SelectedUpcomingProgram.hostname;
                    recgroup.Text = App.ViewModel.SelectedUpcomingProgram.recgroup;

                });
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("Error parsing details: "+ex.ToString()); });
            }

            if (App.ViewModel.appSettings.UseScriptSetting) this.GetPeople();

        }


        private void GetPeople()
        {
            try
                {

                    string query = "SELECT UPPER(`credits`.`role`) AS `role`, ";
                    query += " `people`.`name`, `people`.`person`, ";
                    query += " `videocast`.`intid` AS videoPersonId ";
                    query += " FROM `credits` ";
                    query += " LEFT OUTER JOIN `people` ON `credits`.`person` = `people`.`person` ";
                    query += " LEFT OUTER JOIN `videocast` ON `videocast`.`cast` = `people`.`name` ";
                    query += " WHERE (`credits`.`chanid` = " + App.ViewModel.SelectedUpcomingProgram.chanid;
                    query += " AND `credits`.`starttime` = \"" + App.ViewModel.SelectedUpcomingProgram.starttime.Replace("T", " ") + "\" ) ";
                    query += " ORDER BY `role`,`name` ";

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse&query64=" + Convert.ToBase64String(encoder.GetBytes(query)) + "&rand=" + randText()));
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
                DateTime.TryParse(App.ViewModel.SelectedUpcomingProgram.starttime, out dateResult);

                //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
                TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
                //TimeSpan u = (dateResult - DateTime.Now);
                Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
                //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

                WebBrowserTask webopen = new WebBrowserTask();

                webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedUpcomingProgram.chanid + "/" + timestamp);
                webopen.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening browser.  Check your webserver address in the preferences.");
            }
        }

        private void guideButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.GuideTime = App.ViewModel.SelectedUpcomingProgram.starttime;

            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedTime=" + App.ViewModel.SelectedUpcomingProgram.starttime, UriKind.Relative));

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

            NavigationService.Navigate(new Uri("/People.xaml?Source=upcoming", UriKind.Relative));

            peopleList.SelectedItem = null;
        }
    }
}