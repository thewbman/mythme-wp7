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
    public partial class PeopleProgramDetails : PhoneApplicationPage
    {
        public PeopleProgramDetails()
        {

            InitializeComponent();

            People = new List<PeopleViewModel>();
            encoder = new UTF8Encoding();

            DataContext = App.ViewModel.SelectedPeopleProgram;
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
                string newStartTime = DateTime.Parse(App.ViewModel.SelectedPeopleProgram.starttime).ToUniversalTime().ToString("s");

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetails25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, newStartTime, App.ViewModel.SelectedPeopleProgram.chanid, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Details25Callback), webRequest);
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getDetailsString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedPeopleProgram.starttime, App.ViewModel.SelectedPeopleProgram.chanid, App.ViewModel.randText())));
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

                if (singleProgramElement.Element("Title").FirstNode != null) App.ViewModel.SelectedPeopleProgram.title = (string)singleProgramElement.Element("Title").FirstNode.ToString();
                if (singleProgramElement.Element("SubTitle").FirstNode != null) App.ViewModel.SelectedPeopleProgram.subtitle = (string)singleProgramElement.Element("SubTitle").FirstNode.ToString();

                //App.ViewModel.SelectedPeopleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").FirstNode.ToString();
                if (singleProgramElement.Element("Category").FirstNode != null) App.ViewModel.SelectedPeopleProgram.category = (string)singleProgramElement.Element("Category").FirstNode.ToString();
                if (singleProgramElement.Element("FileSize").FirstNode != null) App.ViewModel.SelectedPeopleProgram.filesize = Int64.Parse((string)singleProgramElement.Element("FileSize").FirstNode.ToString());
                if (singleProgramElement.Element("SeriesId").FirstNode != null) App.ViewModel.SelectedPeopleProgram.seriesid = (string)singleProgramElement.Element("SeriesId").FirstNode.ToString();
                if (singleProgramElement.Element("HostName").FirstNode != null) App.ViewModel.SelectedPeopleProgram.hostname = (string)singleProgramElement.Element("HostName").FirstNode.ToString();
                //App.ViewModel.SelectedPeopleProgram.cattype = (string)singleProgramElement.Element("CatType").FirstNode.ToString();
                if (singleProgramElement.Element("ProgramId").FirstNode != null) App.ViewModel.SelectedPeopleProgram.programid = (string)singleProgramElement.Element("ProgramId").FirstNode.ToString();
                if (singleProgramElement.Element("Season").FirstNode != null) App.ViewModel.SelectedPeopleProgram.season = (string)singleProgramElement.Element("Season").FirstNode.ToString();
                if (singleProgramElement.Element("Episode").FirstNode != null) App.ViewModel.SelectedPeopleProgram.episode = (string)singleProgramElement.Element("Episode").FirstNode.ToString();
                //App.ViewModel.SelectedPeopleProgram.repeat = (string)singleProgramElement.Element("Repeat").FirstNode.ToString();
                //App.ViewModel.SelectedPeopleProgram.stars = (string)singleProgramElement.Element("Stars").FirstNode.ToString();
                if (singleProgramElement.Element("EndTime").FirstNode != null)
                {
                    DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("EndTime").FirstNode.ToString());

                    App.ViewModel.SelectedPeopleProgram.endtime = newEndTime.ToLocalTime().ToString("s");
                    App.ViewModel.SelectedPeopleProgram.endtimespace = newEndTime.ToLocalTime().ToString("s").Replace("T", " ");
                }
                if (singleProgramElement.Element("Airdate").FirstNode != null) App.ViewModel.SelectedPeopleProgram.airdate = (string)singleProgramElement.Element("Airdate").FirstNode.ToString();
                if (singleProgramElement.Element("StartTime").FirstNode != null)
                {
                    DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("StartTime").FirstNode.ToString());

                    App.ViewModel.SelectedPeopleProgram.starttime = newStartTime.ToLocalTime().ToString("s");
                    App.ViewModel.SelectedPeopleProgram.starttimespace = newStartTime.ToLocalTime().ToString("s").Replace("T", " ");
                }
                //App.ViewModel.SelectedPeopleProgram.lastmodified = (string)singleProgramElement.Element("lastModified").FirstNode.ToString();


                App.ViewModel.SelectedPeopleProgram.description = singleProgramElement.Element("Airdate").NextNode.ToString();
                if (singleProgramElement.Element("Description").FirstNode != null) App.ViewModel.SelectedPeopleProgram.description = (string)singleProgramElement.Element("Description").FirstNode.ToString();
                if (App.ViewModel.SelectedPeopleProgram.description.Contains("<Inet")) App.ViewModel.SelectedPeopleProgram.description = "";
                if (App.ViewModel.SelectedPeopleProgram.description.Contains("<Desc")) App.ViewModel.SelectedPeopleProgram.description = "";


                if (App.ViewModel.SelectedPeopleProgram.subtitle == "") App.ViewModel.SelectedPeopleProgram.subtitle = ".";

                if (singleProgramElement.Descendants("Channel").Count() > 0)
                {

                    if (singleProgramElement.Element("Channel").Element("InputId").FirstNode != null) App.ViewModel.SelectedPeopleProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Element("InputId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChannelName").FirstNode != null) App.ViewModel.SelectedPeopleProgram.channame = (string)singleProgramElement.Element("Channel").Element("ChannelName").Value;
                    if (singleProgramElement.Element("Channel").Element("SourceId").FirstNode != null) App.ViewModel.SelectedPeopleProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Element("SourceId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanId").FirstNode != null) App.ViewModel.SelectedPeopleProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Element("ChanId").Value);
                    if (singleProgramElement.Element("Channel").Element("ChanNum").FirstNode != null) App.ViewModel.SelectedPeopleProgram.channum = (string)singleProgramElement.Element("Channel").Element("ChanNum").Value;
                    if (singleProgramElement.Element("Channel").Element("CallSign").FirstNode != null) App.ViewModel.SelectedPeopleProgram.callsign = (string)singleProgramElement.Element("Channel").Element("CallSign").Value;

                }


                if (singleProgramElement.Descendants("Recording").Count() > 0)
                {
                    if (singleProgramElement.Element("Recording").Element("Priority").FirstNode != null) App.ViewModel.SelectedPeopleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Element("Priority").Value);
                    if (singleProgramElement.Element("Recording").Element("Status").FirstNode != null) App.ViewModel.SelectedPeopleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Element("Status").Value);
                    //App.ViewModel.SelectedPeopleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedPeopleProgram.recstatus);
                    if (singleProgramElement.Element("Recording").Element("RecGroup").FirstNode != null) App.ViewModel.SelectedPeopleProgram.recgroup = (string)singleProgramElement.Element("Recording").Element("RecGroup").Value;
                    //if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null) App.ViewModel.SelectedPeopleProgram.recstartts = (string)singleProgramElement.Element("Recording").Element("StartTs").Value;
                    //if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null) App.ViewModel.SelectedPeopleProgram.recendts = (string)singleProgramElement.Element("Recording").Element("EndTs").Value;
                    if (singleProgramElement.Element("Recording").Element("RecordId").FirstNode != null) App.ViewModel.SelectedPeopleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Element("RecordId").Value);

                    if (singleProgramElement.Element("Recording").Element("StartTs").FirstNode != null)
                    {
                        DateTime newStartTime = DateTime.Parse((string)singleProgramElement.Element("Recording").Element("StartTs").FirstNode.ToString());

                        App.ViewModel.SelectedPeopleProgram.recstartts = newStartTime.ToLocalTime().ToString("s");
                    }

                    if (singleProgramElement.Element("Recording").Element("EndTs").FirstNode != null)
                    {
                        DateTime newEndTime = DateTime.Parse((string)singleProgramElement.Element("Recording").Element("EndTs").FirstNode.ToString());

                        App.ViewModel.SelectedPeopleProgram.recendts = newEndTime.ToLocalTime().ToString("s");
                    }
                }
                else
                {
                    App.ViewModel.SelectedPeopleProgram.recstatus = -20;
                }

                App.ViewModel.SelectedPeopleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedPeopleProgram.recstatus);


                if (App.ViewModel.SelectedPeopleProgram.recstatus == -2)
                {
                    App.ViewModel.SelectedPeopleProgram.recordedfourthline = "Currently recording (" + App.ViewModel.SelectedPeopleProgram.channum + " - " + App.ViewModel.SelectedPeopleProgram.channame + ")";
                }
                else
                {
                    App.ViewModel.SelectedPeopleProgram.recordedfourthline = App.ViewModel.SelectedPeopleProgram.channum + " - " + App.ViewModel.SelectedPeopleProgram.channame;
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
                                App.ViewModel.SelectedPeopleProgram.coverart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                break;
                            case "fanart":
                                App.ViewModel.SelectedPeopleProgram.fanart = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
                                break;
                            case "banner":
                                App.ViewModel.SelectedPeopleProgram.banner = arturlbase + singleArtworkInfoElement.Element("URL").FirstNode.ToString() + arturlend;
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

                    DataContext = App.ViewModel.SelectedPeopleProgram;
                    topPanorama.DataContext = App.ViewModel.SelectedPeopleProgram;

                    recstatustext.Text = App.ViewModel.SelectedPeopleProgram.recstatustext;
                    title.Text = App.ViewModel.SelectedPeopleProgram.title;
                    subtitle.Text = App.ViewModel.SelectedPeopleProgram.subtitle;
                    category.Text = App.ViewModel.SelectedPeopleProgram.category;
                    description.Text = App.ViewModel.SelectedPeopleProgram.description;

                    starttime.Text = App.ViewModel.SelectedPeopleProgram.starttime;
                    endtime.Text = App.ViewModel.SelectedPeopleProgram.endtime;
                    seriesid.Text = App.ViewModel.SelectedPeopleProgram.seriesid;
                    programid.Text = App.ViewModel.SelectedPeopleProgram.programid;
                    airdate.Text = App.ViewModel.SelectedPeopleProgram.airdate;
                    season.Text = App.ViewModel.SelectedPeopleProgram.season;
                    episode.Text = App.ViewModel.SelectedPeopleProgram.episode;

                    channum.Text = App.ViewModel.SelectedPeopleProgram.channum;
                    channame.Text = App.ViewModel.SelectedPeopleProgram.channame;
                    chanid.Text = App.ViewModel.SelectedPeopleProgram.chanid.ToString();

                    recstatustext2.Text = App.ViewModel.SelectedPeopleProgram.recstatustext;
                    recstartts.Text = App.ViewModel.SelectedPeopleProgram.recstartts;
                    recendts.Text = App.ViewModel.SelectedPeopleProgram.recendts;
                    //hostname.Text = App.ViewModel.SelectedPeopleProgram.hostname;
                    recgroup.Text = App.ViewModel.SelectedPeopleProgram.recgroup;

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
                    App.ViewModel.SelectedPeopleProgram.title = (string)singleProgramElement.Attribute("title").Value;
                    App.ViewModel.SelectedPeopleProgram.subtitle = (string)singleProgramElement.Attribute("subTitle").Value;

                    //App.ViewModel.SelectedPeopleProgram.programflags = (string)singleProgramElement.Attribute("programFlags").Value;
                    App.ViewModel.SelectedPeopleProgram.category = (string)singleProgramElement.Attribute("category").Value;
                    if (singleProgramElement.Attributes("fileSize").Count() > 0) App.ViewModel.SelectedPeopleProgram.filesize = Int64.Parse((string)singleProgramElement.Attribute("fileSize").Value);
                    App.ViewModel.SelectedPeopleProgram.seriesid = (string)singleProgramElement.Attribute("seriesId").Value;
                    App.ViewModel.SelectedPeopleProgram.hostname = (string)singleProgramElement.Attribute("hostname").Value;
                    //App.ViewModel.SelectedPeopleProgram.cattype = (string)singleProgramElement.Attribute("catType").Value;
                    App.ViewModel.SelectedPeopleProgram.programid = (string)singleProgramElement.Attribute("programId").Value;
                    //App.ViewModel.SelectedPeopleProgram.repeat = (string)singleProgramElement.Attribute("repeat").Value;
                    //App.ViewModel.SelectedPeopleProgram.stars = (string)singleProgramElement.Attribute("stars").Value;
                    App.ViewModel.SelectedPeopleProgram.endtime = (string)singleProgramElement.Attribute("endTime").Value;
                    App.ViewModel.SelectedPeopleProgram.endtimespace = (string)singleProgramElement.Attribute("endTime").Value.Replace("T", " ");
                    if (singleProgramElement.Attributes("airdate").Count() > 0) App.ViewModel.SelectedPeopleProgram.airdate = (string)singleProgramElement.Attribute("airdate").Value;
                    App.ViewModel.SelectedPeopleProgram.starttime = (string)singleProgramElement.Attribute("startTime").Value;
                    App.ViewModel.SelectedPeopleProgram.starttimespace = (string)singleProgramElement.Attribute("startTime").Value.Replace("T", " ");
                    //App.ViewModel.SelectedPeopleProgram.lastmodified = (string)singleProgramElement.Attribute("lastModified").Value;

                    App.ViewModel.SelectedPeopleProgram.description = (string)singleProgramElement.FirstNode.ToString();

                    if (App.ViewModel.SelectedPeopleProgram.description.Contains("<Channel")) App.ViewModel.SelectedPeopleProgram.description = "";
                    if (App.ViewModel.SelectedPeopleProgram.description.Contains("<Recording")) App.ViewModel.SelectedPeopleProgram.description = "";


                    if (singleProgramElement.Descendants("Channel").Count() > 0)
                    {
                        App.ViewModel.SelectedPeopleProgram.inputid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("inputId").Value);
                        App.ViewModel.SelectedPeopleProgram.channame = (string)singleProgramElement.Element("Channel").Attribute("channelName").Value;
                        App.ViewModel.SelectedPeopleProgram.sourceid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("sourceId").Value);
                        App.ViewModel.SelectedPeopleProgram.chanid = int.Parse((string)singleProgramElement.Element("Channel").Attribute("chanId").Value);
                        App.ViewModel.SelectedPeopleProgram.channum = (string)singleProgramElement.Element("Channel").Attribute("chanNum").Value;
                        App.ViewModel.SelectedPeopleProgram.callsign = (string)singleProgramElement.Element("Channel").Attribute("callSign").Value;
                    }


                    if (singleProgramElement.Descendants("Recording").Count() > 0)
                    {
                        App.ViewModel.SelectedPeopleProgram.recpriority = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recPriority").Value);
                        App.ViewModel.SelectedPeopleProgram.recstatus = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recStatus").Value);
                        //App.ViewModel.SelectedPeopleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedPeopleProgram.recstatus);
                        App.ViewModel.SelectedPeopleProgram.recgroup = (string)singleProgramElement.Element("Recording").Attribute("recGroup").Value;
                        App.ViewModel.SelectedPeopleProgram.recstartts = (string)singleProgramElement.Element("Recording").Attribute("recStartTs").Value;
                        App.ViewModel.SelectedPeopleProgram.recendts = (string)singleProgramElement.Element("Recording").Attribute("recEndTs").Value;
                        App.ViewModel.SelectedPeopleProgram.recordid = int.Parse((string)singleProgramElement.Element("Recording").Attribute("recordId").Value);

                    }
                    else
                    {
                        App.ViewModel.SelectedPeopleProgram.recstatus = -20;
                    }

                    App.ViewModel.SelectedPeopleProgram.recstatustext = App.ViewModel.functions.RecStatusDecode(App.ViewModel.SelectedPeopleProgram.recstatus);


                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Done updating");

                    DataContext = App.ViewModel.SelectedPeopleProgram;
                    topPanorama.DataContext = App.ViewModel.SelectedPeopleProgram;

                    recstatustext.Text = App.ViewModel.SelectedPeopleProgram.recstatustext;
                    title.Text = App.ViewModel.SelectedPeopleProgram.title;
                    subtitle.Text = App.ViewModel.SelectedPeopleProgram.subtitle;
                    category.Text = App.ViewModel.SelectedPeopleProgram.category;
                    description.Text = App.ViewModel.SelectedPeopleProgram.description;

                    starttime.Text = App.ViewModel.SelectedPeopleProgram.starttime;
                    endtime.Text = App.ViewModel.SelectedPeopleProgram.endtime;
                    seriesid.Text = App.ViewModel.SelectedPeopleProgram.seriesid;
                    programid.Text = App.ViewModel.SelectedPeopleProgram.programid;
                    airdate.Text = App.ViewModel.SelectedPeopleProgram.airdate;

                    channum.Text = App.ViewModel.SelectedPeopleProgram.channum;
                    channame.Text = App.ViewModel.SelectedPeopleProgram.channame;
                    chanid.Text = App.ViewModel.SelectedPeopleProgram.chanid.ToString();

                    recstatustext2.Text = App.ViewModel.SelectedPeopleProgram.recstatustext;
                    recstartts.Text = App.ViewModel.SelectedPeopleProgram.recstartts;
                    recendts.Text = App.ViewModel.SelectedPeopleProgram.recendts;
                    //hostname.Text = App.ViewModel.SelectedPeopleProgram.hostname;
                    recgroup.Text = App.ViewModel.SelectedPeopleProgram.recgroup;

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
                query += " WHERE (`credits`.`chanid` = " + App.ViewModel.SelectedPeopleProgram.chanid;
                query += " AND `credits`.`starttime` = \"" + App.ViewModel.SelectedPeopleProgram.starttime.Replace("T", " ") + "\" ) ";
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
                DateTime.TryParse(App.ViewModel.SelectedPeopleProgram.starttime, out dateResult);

                //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
                TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
                //TimeSpan u = (dateResult - DateTime.Now);
                Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
                //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

                WebBrowserTask webopen = new WebBrowserTask();

                webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedPeopleProgram.chanid + "/" + timestamp);
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

            NavigationService.Navigate(new Uri("/People.xaml?Source=people", UriKind.Relative));

            peopleList.SelectedItem = null;
        }

        private void guidebutton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedTime=" + App.ViewModel.SelectedPeopleProgram.starttime.Replace("asdf", ""), UriKind.Relative));
        }

        private void titleSearchButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.SelectedTitle = App.ViewModel.SelectedPeopleProgram.title;

            NavigationService.Navigate(new Uri("/Search.xaml?Source=people", UriKind.Relative));
        }

        private void setupSchedulebutton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.SelectedSetupProgram = App.ViewModel.SelectedPeopleProgram;

            NavigationService.Navigate(new Uri("/SetupSchedule.xaml?Source=people", UriKind.Relative));
        }
    }
}