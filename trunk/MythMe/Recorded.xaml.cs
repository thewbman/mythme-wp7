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
    public partial class Recorded : PhoneApplicationPage
    {
        public Recorded()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            AllRecordedListBox.ItemsSource = AllRecorded;
            DefaultRecordedListBox.ItemsSource = DefaultRecorded;
            DeletedRecordedListBox.ItemsSource = DeletedRecorded;
            LiveTVRecordedListBox.ItemsSource = LiveTVRecorded;
        }

        private string getRecorded25String = "http://{0}:{1}/Dvr/GetRecorded?random={2}";
        private string getRecordedString = "http://{0}:{1}/Myth/GetRecorded?random={2}";

        ObservableCollection<ProgramViewModel> AllRecorded = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> DefaultRecorded = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> DeletedRecorded = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> LiveTVRecorded = new ObservableCollection<ProgramViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (App.ViewModel.Recorded.Count == 0) this.Perform(() => GetRecorded(), 5000);
            else
            {
                /*
                AllRecorded.Clear();
                DefaultRecorded.Clear();
                DeletedRecorded.Clear();
                LiveTVRecorded.Clear();

                foreach (ProgramViewModel singleProgram in App.ViewModel.Recorded)
                {
                    if (singleProgram.recgroup == "Default") DefaultRecorded.Add(singleProgram);
                    else if (singleProgram.recgroup == "Deleted") DeletedRecorded.Add(singleProgram);
                    else if (singleProgram.recgroup == "LiveTV") LiveTVRecorded.Add(singleProgram);
                }
                
                AllTitle.Header = "All (" + App.ViewModel.Recorded.Count + ")";
                DefaultTitle.Header = "Default (" + DefaultRecorded.Count + ")";
                DeletedTitle.Header = "Deleted (" + DeletedRecorded.Count + ")";
                LiveTVTitle.Header = "LiveTV (" + LiveTVRecorded.Count + ")";

                AllRecordedListBox.ItemsSource = App.ViewModel.Recorded;
                DefaultRecordedListBox.ItemsSource = DefaultRecorded;
                DeletedRecordedListBox.ItemsSource = DeletedRecorded;
                LiveTVRecordedListBox.ItemsSource = LiveTVRecorded;
                 */

                SortAndDisplay();

            }
        }

        private void GetRecorded()
        {

            performanceProgressBarCustomized.IsIndeterminate = true;

            AllTitle.Header = "All";
            DefaultTitle.Header = "Default";
            DeletedTitle.Header = "Deleted";
            LiveTVTitle.Header = "LiveTV";

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
            }
        }

        private void Recorded25Callback(IAsyncResult asynchronousResult)
        {
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


                foreach (XElement singleRecordedElement in xdoc.Element("GetRecordedResponse").Element("Recorded").Element("Programs").Descendants("Program")) 
                {
                    ProgramViewModel singleRecorded = new ProgramViewModel() { };

                    singleRecorded.title = (string)singleRecordedElement.Attribute("title").Value;
                    singleRecorded.subtitle = (string)singleRecordedElement.Attribute("subTitle").Value;

                    //singleRecorded.programflags = (string)singleRecordedElement.Attribute("programFlags").Value;
                    singleRecorded.category = (string)singleRecordedElement.Attribute("category").Value;
                    //singleRecorded.filesize = int.Parse((string)singleRecordedElement.Attribute("fileSize").Value);
                    singleRecorded.seriesid = (string)singleRecordedElement.Attribute("seriesId").Value;
                    singleRecorded.hostname = (string)singleRecordedElement.Attribute("hostname").Value;
                    //singleRecorded.cattype = (string)singleRecordedElement.Attribute("catType").Value;
                    singleRecorded.programid = (string)singleRecordedElement.Attribute("programId").Value;
                    //singleRecorded.repeat = (string)singleRecordedElement.Attribute("repeat").Value;
                    //singleRecorded.stars = (string)singleRecordedElement.Attribute("stars").Value;
                    singleRecorded.endtime = (string)singleRecordedElement.Attribute("endTime").Value;
                    singleRecorded.endtimespace = (string)singleRecordedElement.Attribute("endTime").Value.Replace("T", " ");
                    if (singleRecordedElement.Attributes("airdate").Count() > 0) singleRecorded.airdate = (string)singleRecordedElement.Attribute("airdate").Value;
                    singleRecorded.starttime = (string)singleRecordedElement.Attribute("startTime").Value;
                    singleRecorded.starttimespace = (string)singleRecordedElement.Attribute("startTime").Value.Replace("T"," ");
                    //singleRecorded.lastmodified = (string)singleRecordedElement.Attribute("lastModified").Value;
                    
                    singleRecorded.inputid = int.Parse((string)singleRecordedElement.Element("Channel").Attribute("inputId").Value);
                    singleRecorded.channame = (string)singleRecordedElement.Element("Channel").Attribute("channelName").Value;
                    singleRecorded.sourceid = int.Parse((string)singleRecordedElement.Element("Channel").Attribute("sourceId").Value);
                    singleRecorded.chanid = int.Parse((string)singleRecordedElement.Element("Channel").Attribute("chanId").Value);
                    singleRecorded.channum = (string)singleRecordedElement.Element("Channel").Attribute("chanNum").Value;
                    singleRecorded.callsign = (string)singleRecordedElement.Element("Channel").Attribute("callSign").Value;
                    /*
                    */

                    singleRecorded.recpriority = int.Parse((string)singleRecordedElement.Element("Recording").Attribute("recPriority").Value);
                    singleRecorded.recstatus = int.Parse((string)singleRecordedElement.Element("Recording").Attribute("recStatus").Value);
                    singleRecorded.recgroup = (string)singleRecordedElement.Element("Recording").Attribute("recGroup").Value;
                    singleRecorded.recstartts = (string)singleRecordedElement.Element("Recording").Attribute("recStartTs").Value;
                    singleRecorded.recordid = int.Parse((string)singleRecordedElement.Element("Recording").Attribute("recordId").Value);


                    //replace with real logic and function
                    //singleRecorded.screenshot = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetPreviewImage?ChanId=";
                    //singleRecorded.screenshot += singleRecorded.chanid + "&StartTime=" + singleRecorded.recstartts.Replace("T", " ");
                    singleRecorded.screenshot = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + "/cgi-bin/webmyth.py?op=getPremadeImage&chanid=";
                    singleRecorded.screenshot += singleRecorded.chanid + "&starttime=" + singleRecorded.recstartts.Replace("T", " ");

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {   
                        //programlist.Add(singleRecorded);
                        App.ViewModel.Recorded.Add(singleRecorded);

                        /*
                        if (singleRecorded.recgroup == "Default")
                        {
                            DefaultRecorded.Add(singleRecorded);
                           // DefaultRecorded.OrderBy(p => p.title);
                        }
                        else if (singleRecorded.recgroup == "Deleted")
                        {
                            DeletedRecorded.Add(singleRecorded);
                            //DeletedRecorded.OrderBy(p => p.title);
                        }
                        else if (singleRecorded.recgroup == "LiveTV")
                        {
                            LiveTVRecorded.Add(singleRecorded);
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

            switch (App.ViewModel.appSettings.RecSortSetting)
            {
                case "date":
                    foreach (var item in App.ViewModel.Recorded)
                    {
                        item.recsort = item.starttime;
                    }
                    break;
                case "airdate":
                    foreach (var item in App.ViewModel.Recorded)
                    {
                        item.recsort = item.airdate;
                    }
                    break;
                case "title":
                    foreach (var item in App.ViewModel.Recorded)
                    {
                        item.recsort = item.title;
                    }
                    break;
                default:
                    foreach (var item in App.ViewModel.Recorded)
                    {
                        item.recsort = item.title;
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

            AllTitle.Header = "All (" + AllRecorded.Count + ")";
            DefaultTitle.Header = "Default (" + DefaultRecorded.Count + ")";
            DeletedTitle.Header = "Deleted (" + DeletedRecorded.Count + ")";
            LiveTVTitle.Header = "LiveTV (" + LiveTVRecorded.Count + ")";

            performanceProgressBarCustomized.IsIndeterminate = false;

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
            if (this.SortPopup.Parent == null)
            {
                this.SortPopup.IsOpen = true;
            }
        }

        private void SortItem_Click(object sender, RoutedEventArgs e)
        {
            string header = (sender as MenuItem).Header.ToString();

            switch (header)
            {
                case "Date, Asc":
                    App.ViewModel.appSettings.RecSortSetting = "date";
                    App.ViewModel.appSettings.RecSortAscSetting = true;
                    break;
                case "Date, Desc":
                    App.ViewModel.appSettings.RecSortSetting = "date";
                    App.ViewModel.appSettings.RecSortAscSetting = false;
                    break;
                case "Original airdate, Asc":
                    App.ViewModel.appSettings.RecSortSetting = "airdate";
                    App.ViewModel.appSettings.RecSortAscSetting = true;
                    break;
                case "Original airdate, Desc":
                    App.ViewModel.appSettings.RecSortSetting = "airdate";
                    App.ViewModel.appSettings.RecSortAscSetting = false;
                    break;
                case "Title, Asc":
                    App.ViewModel.appSettings.RecSortSetting = "title";
                    App.ViewModel.appSettings.RecSortAscSetting = true;
                    break;
                case "Title, Desc":
                    App.ViewModel.appSettings.RecSortSetting = "title";
                    App.ViewModel.appSettings.RecSortAscSetting = false;
                    break;
                default:
                    App.ViewModel.appSettings.RecSortSetting = "date";
                    App.ViewModel.appSettings.RecSortAscSetting = false;
                    break;
            }

            SortAndDisplay();
        }
    }
}