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

            People = new List<PeopleModel>();
            encoder = new UTF8Encoding();

            DataContext = App.ViewModel.SelectedProgram;
        }

        List<PeopleModel> People;
        UTF8Encoding encoder;

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
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetails25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedProgram.starttime, App.ViewModel.SelectedProgram.chanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Details25Callback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetailsString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedProgram.starttime, App.ViewModel.SelectedProgram.chanid, App.ViewModel.randText())));
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

                if (singleProgramElement.Element("Title").FirstNode != null) App.ViewModel.SelectedProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                if (singleProgramElement.Element("SubTitle").FirstNode != null) App.ViewModel.SelectedProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                //App.ViewModel.SelectedProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                if (singleProgramElement.Element("Category").FirstNode != null) App.ViewModel.SelectedProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                if (singleProgramElement.Element("FileSize").FirstNode != null) App.ViewModel.SelectedProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                if (singleProgramElement.Element("SeriesId").FirstNode != null) App.ViewModel.SelectedProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                if (singleProgramElement.Element("Hostname").FirstNode != null) App.ViewModel.SelectedProgram.hostname = (string)singleProgramElement.Element("Hostname").FirstNode.ToString();
                //App.ViewModel.SelectedProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                if (singleProgramElement.Element("ProgramId").FirstNode != null) App.ViewModel.SelectedProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                //App.ViewModel.SelectedProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                //App.ViewModel.SelectedProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                if (singleProgramElement.Element("EndTime").FirstNode != null) App.ViewModel.SelectedProgram.endtime = (string)singleProgramElement.Element("EndTime").FirstNode.ToString();
                if (singleProgramElement.Element("EndTime").FirstNode != null) App.ViewModel.SelectedProgram.endtimespace = (string)singleProgramElement.Element("EndTime").FirstNode.ToString().Replace("T", " ");
                if (singleProgramElement.Element("Airdate").FirstNode != null) App.ViewModel.SelectedProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                if (singleProgramElement.Element("StartTime").FirstNode != null) App.ViewModel.SelectedProgram.starttime = (string)singleProgramElement.Element("StartTime").FirstNode.ToString();
                if (singleProgramElement.Element("StartTime").FirstNode != null) App.ViewModel.SelectedProgram.starttimespace = (string)singleProgramElement.Element("StartTime").FirstNode.ToString().Replace("T", " ");
                //App.ViewModel.SelectedProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();
                

                App.ViewModel.SelectedProgram.description = singleProgramElement.Element("Airdate").NextNode.ToString();
                if (App.ViewModel.SelectedProgram.description.Contains("<Inet")) App.ViewModel.SelectedProgram.description = "";


                if (App.ViewModel.SelectedProgram.subtitle == "") App.ViewModel.SelectedProgram.subtitle = ".";

                if (singleProgramElement.Descendants("Channel").Count() > 0)
                {

                    if (singleProgramElement.Element("Channel").Element("InputId").FirstNode != null) App.ViewModel.SelectedProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Element("InputId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChannelName").FirstNode != null) App.ViewModel.SelectedProgram.channame = (string)singleProgramElement.Element("Channel").Element("ChannelName").Value;
                    if (singleProgramElement.Element("Channel").Element("SourceId").FirstNode != null) App.ViewModel.SelectedProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Element("SourceId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanId").FirstNode != null) App.ViewModel.SelectedProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Element("ChanId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanNum").FirstNode != null) App.ViewModel.SelectedProgram.channum = (string)singleProgramElement.Element("Channel").Element("ChanNum").Value;
                    if (singleProgramElement.Element("Channel").Element("CallSign").FirstNode != null) App.ViewModel.SelectedProgram.callsign = (string)singleProgramElement.Element("Channel").Element("CallSign").Value;

                }


                if (singleProgramElement.Descendants("Recording").Count() > 0)
                {
                    if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) App.ViewModel.SelectedProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                    if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) App.ViewModel.SelectedProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                    //App.ViewModel.SelectedProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedProgram.recstatus);
                    if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) App.ViewModel.SelectedProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                    if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) App.ViewModel.SelectedProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                    if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) App.ViewModel.SelectedProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                    if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) App.ViewModel.SelectedProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                }
                else
                {
                    App.ViewModel.SelectedProgram.recstatus = -20;
                }

                App.ViewModel.SelectedProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedProgram.recstatus);


                if (App.ViewModel.SelectedProgram.recstatus == -2)
                {
                    App.ViewModel.SelectedProgram.recordedfourthline = "Currently recording (" + App.ViewModel.SelectedProgram.channum + " - " + App.ViewModel.SelectedProgram.channame + ")";
                }
                else
                {
                    App.ViewModel.SelectedProgram.recordedfourthline = App.ViewModel.SelectedProgram.channum + " - " + App.ViewModel.SelectedProgram.channame;
                }




                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Done updating");

                    DataContext = App.ViewModel.SelectedProgram;
                    topPanorama.DataContext = App.ViewModel.SelectedProgram;

                    recstatustext.Text = App.ViewModel.SelectedProgram.recstatustext;
                    title.Text = App.ViewModel.SelectedProgram.title;
                    subtitle.Text = App.ViewModel.SelectedProgram.subtitle;
                    category.Text = App.ViewModel.SelectedProgram.category;
                    description.Text = App.ViewModel.SelectedProgram.description;

                    starttime.Text = App.ViewModel.SelectedProgram.starttime;
                    endtime.Text = App.ViewModel.SelectedProgram.endtime;
                    seriesid.Text = App.ViewModel.SelectedProgram.seriesid;
                    programid.Text = App.ViewModel.SelectedProgram.programid;
                    airdate.Text = App.ViewModel.SelectedProgram.airdate;

                    channum.Text = App.ViewModel.SelectedProgram.channum;
                    channame.Text = App.ViewModel.SelectedProgram.channame;
                    chanid.Text = App.ViewModel.SelectedProgram.chanid.ToString();

                    recstatustext2.Text = App.ViewModel.SelectedProgram.recstatustext;
                    recstartts.Text = App.ViewModel.SelectedProgram.recstartts;
                    recendts.Text = App.ViewModel.SelectedProgram.recendts;
                    //hostname.Text = App.ViewModel.SelectedProgram.hostname;
                    recgroup.Text = App.ViewModel.SelectedProgram.recgroup;

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

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Done updating");

                    DataContext = App.ViewModel.SelectedProgram;
                    topPanorama.DataContext = App.ViewModel.SelectedProgram;

                    recstatustext.Text = App.ViewModel.SelectedProgram.recstatustext;
                    title.Text = App.ViewModel.SelectedProgram.title;
                    subtitle.Text = App.ViewModel.SelectedProgram.subtitle;
                    category.Text = App.ViewModel.SelectedProgram.category;
                    description.Text = App.ViewModel.SelectedProgram.description;

                    starttime.Text = App.ViewModel.SelectedProgram.starttime;
                    endtime.Text = App.ViewModel.SelectedProgram.endtime;
                    seriesid.Text = App.ViewModel.SelectedProgram.seriesid;
                    programid.Text = App.ViewModel.SelectedProgram.programid;
                    airdate.Text = App.ViewModel.SelectedProgram.airdate;

                    channum.Text = App.ViewModel.SelectedProgram.channum;
                    channame.Text = App.ViewModel.SelectedProgram.channame;
                    chanid.Text = App.ViewModel.SelectedProgram.chanid.ToString();

                    recstatustext2.Text = App.ViewModel.SelectedProgram.recstatustext;
                    recstartts.Text = App.ViewModel.SelectedProgram.recstartts;
                    recendts.Text = App.ViewModel.SelectedProgram.recendts;
                    //hostname.Text = App.ViewModel.SelectedProgram.hostname;
                    recgroup.Text = App.ViewModel.SelectedProgram.recgroup;

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
                    query += " WHERE (`credits`.`chanid` = " + App.ViewModel.SelectedProgram.chanid;
                    query += " AND `credits`.`starttime` = \"" + App.ViewModel.SelectedProgram.starttime.Replace("T", " ") + "\" ) ";
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
                //List<PeopleModel> lp = new List<PeopleModel>();

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<PeopleModel>));

                People = (List<PeopleModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got people: " + PeopleModel.Count);

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
                DateTime.TryParse(App.ViewModel.SelectedProgram.starttime, out dateResult);

                //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
                TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
                //TimeSpan u = (dateResult - DateTime.Now);
                Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
                //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

                WebBrowserTask webopen = new WebBrowserTask();

                webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedProgram.chanid + "/" + timestamp);
                webopen.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening browser.  Check your webserver address in the preferences.");
            }
        }

        private void guideButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.GuideTime = App.ViewModel.SelectedProgram.starttime;

            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedTime=" + App.ViewModel.SelectedProgram.starttime, UriKind.Relative));

        }


        private static string randText()
        {
            Random random = new Random();

            return random.Next().ToString();
        }
    }
}