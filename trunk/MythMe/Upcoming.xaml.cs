using System;
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

            AllUpcomingListBox.ItemsSource = AllUpcoming;
            ConflictingUpcomingListBox.ItemsSource = ConflictingUpcoming;
            OverridesUpcomingListBox.ItemsSource = OverridesUpcoming;
            UpcomingUpcomingListBox.ItemsSource = UpcomingUpcoming;
        }

        const int MAX_BUFFER_SIZE = 1460;   //this is the payload size sent from my backend 0.24.1+fixes

        private int protocolStatus;
        private int protocolResponseLength;
        private string fullProtocolResponse;

        ObservableCollection<ProgramViewModel> AllUpcoming = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> ConflictingUpcoming = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> OverridesUpcoming = new ObservableCollection<ProgramViewModel>();
        ObservableCollection<ProgramViewModel> UpcomingUpcoming = new ObservableCollection<ProgramViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (App.ViewModel.Upcoming.Count == 0) this.Perform(() => GetUpcoming(), 100);
            else
            {

                SortAndDisplay("");

            }

        }
        
        private void GetUpcoming()
        {

            performanceProgressBarCustomized.IsIndeterminate = true;

            AllUpcoming.Clear();
            ConflictingUpcoming.Clear();
            OverridesUpcoming.Clear();
            UpcomingUpcoming.Clear();

            App.ViewModel.Upcoming.Clear();

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
                throw new SocketException((int)e.SocketError);
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
                throw new SocketException((int)e.SocketError);
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
                            throw new Exception("Did not get ACCEPT back: "+newResponse);
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
                            throw new Exception("Did not get OK back from ANN: " + newResponse);
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
                        throw new Exception("Unknown protocolStatus (" + protocolStatus + ") and response: " + newResponse);
                        break;
                }

            }
            else
            {
                throw new SocketException((int)e.SocketError);
            }
        }


        private void ParseUpcoming()
        {
            //MessageBox.Show("ready to parse upcoming of length: " + fullProtocolResponse.Length);

            if (App.ViewModel.appSettings.ProtoVerSetting == 23056)
                ParseUpcoming41();
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
        private void ParseUpcoming67()
        {
            MessageBox.Show("Your MythTV protocol version (" + App.ViewModel.appSettings.ProtoVerSetting + ") is not currently supported by MythMe");
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
                MessageBox.Show("Problems reading upcoming, not all upcoming programs will be visible", "Error", MessageBoxButton.OK);
            }

            //upcomingHeader.Title = "upcoming: " + App.ViewModel.Upcoming.Count + " " + programIndex + " " + responseArray[1] + " " + responseArray.Length + " " + (Int64.Parse(responseArray[1])*41+2);

            SortAndDisplay(responseArray[0].Substring(8).Trim());
        }
        private void ParseUpcoming41()
        {
            MessageBox.Show("Your MythTV protocol version (" + App.ViewModel.appSettings.ProtoVerSetting + ") is not currently supported by MythMe");
        }
        private void ParseUpcoming35()
        {
            MessageBox.Show("Your MythTV protocol version (" + App.ViewModel.appSettings.ProtoVerSetting + ") is not currently supported by MythMe");
        }
        private void ParseUpcoming32()
        {
            MessageBox.Show("Your MythTV protocol version (" + App.ViewModel.appSettings.ProtoVerSetting + ") is not currently supported by MythMe");
        }
        private void ParseUpcoming31()
        {
            MessageBox.Show("Your MythTV protocol version (" + App.ViewModel.appSettings.ProtoVerSetting + ") is not currently supported by MythMe");
        }
        private void ParseUpcoming25()
        {
            MessageBox.Show("Your MythTV protocol version (" + App.ViewModel.appSettings.ProtoVerSetting + ") is not currently supported by MythMe");
        }


        private void SortAndDisplay(string inConflicting)
        {

            AllUpcoming.Clear();
            ConflictingUpcoming.Clear();
            OverridesUpcoming.Clear();
            UpcomingUpcoming.Clear();

            //sorting

            foreach (var item in App.ViewModel.Upcoming)
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

            AllTitle.Header = "All (" + AllUpcoming.Count + ")";
            ConflictingTitle.Header = "Conflicting (" + ConflictingUpcoming.Count + ")";
            OverridesTitle.Header = "Overrides (" + OverridesUpcoming.Count + ")";
            UpcomingTitle.Header = "Upcoming (" + UpcomingUpcoming.Count + ")";

            performanceProgressBarCustomized.IsIndeterminate = false;

            if (inConflicting == "1") BannerMessage("There are conflicting scheduled recordings");
            
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

        private void Perform(Action myMethod, int delayInMilliseconds)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (s, e) => Thread.Sleep(delayInMilliseconds);

            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();

            worker.RunWorkerAsync();
        }

        private void UpcomingUpcomingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (UpcomingUpcomingListBox.SelectedItem == null)
                return;

            App.ViewModel.SelectedProgram = (ProgramViewModel)UpcomingUpcomingListBox.SelectedItem;

            NavigationService.Navigate(new Uri("/UpcomingDetails.xaml", UriKind.Relative));

            UpcomingUpcomingListBox.SelectedItem = null;

        }

        private void AllUpcomingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (AllUpcomingListBox.SelectedItem == null)
                return;

            App.ViewModel.SelectedProgram = (ProgramViewModel)AllUpcomingListBox.SelectedItem;

            NavigationService.Navigate(new Uri("/UpcomingDetails.xaml", UriKind.Relative));

            AllUpcomingListBox.SelectedItem = null;

        }

        private void ConflictingUpcomingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ConflictingUpcomingListBox.SelectedItem == null)
                return;

            App.ViewModel.SelectedProgram = (ProgramViewModel)ConflictingUpcomingListBox.SelectedItem;

            NavigationService.Navigate(new Uri("/UpcomingDetails.xaml", UriKind.Relative));

            ConflictingUpcomingListBox.SelectedItem = null;

        }

        private void OverridesUpcomingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (OverridesUpcomingListBox.SelectedItem == null)
                return;

            App.ViewModel.SelectedProgram = (ProgramViewModel)OverridesUpcomingListBox.SelectedItem;

            NavigationService.Navigate(new Uri("/UpcomingDetails.xaml", UriKind.Relative));

            OverridesUpcomingListBox.SelectedItem = null;
        }
    }
}