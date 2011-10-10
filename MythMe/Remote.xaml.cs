using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace MythMe
{
    public partial class Remote : PhoneApplicationPage
    {
        public Remote()
        {
            InitializeComponent();

            connected = false;
            //remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //remoteSocketEventArg = new SocketAsyncEventArgs();
            //remoteAddress = "192.168.1.104";
            //remotePort = 6546; 
            //remoteEndPoint = new DnsEndPoint(remoteAddress, remotePort);

            currentFrontend = new FrontendViewModel();

            this.responseText = "";
            
        }

        const int MAX_BUFFER_SIZE = 2048;

        private bool connected;
        private Socket remoteSocket;
        SocketAsyncEventArgs remoteSocketEventArg;
        private string remoteAddress;
        private int remotePort;
        private DnsEndPoint remoteEndPoint;

        private FrontendViewModel currentFrontend;


        public string responseText
        {
            get { return responseText; }
            set {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    remoteResponse.Text = value;
                }); 
            }
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            currentFrontend = App.ViewModel.Frontends[App.ViewModel.appSettings.RemoteIndexSetting];

            if (currentFrontend.Address == null) currentFrontend.Address = App.ViewModel.appSettings.MasterBackendIpSetting;

            remoteEndPoint = new DnsEndPoint(currentFrontend.Address, currentFrontend.Port);

            remotePivot.Title = "remote: " + currentFrontend.Name + " @ "+currentFrontend.Address;

            remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
            remoteSocketEventArg = new SocketAsyncEventArgs();
            

            if(connected == false) {

                remoteSocketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
                
                remoteSocketEventArg.RemoteEndPoint = remoteEndPoint;
                remoteSocketEventArg.UserToken = remoteSocket;

                remoteSocket.ConnectAsync(remoteSocketEventArg);


                connected = true;
               
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            remoteSocket.Close();

            remoteSocket.Dispose();

            connected = false;
            
            base.OnNavigatedFrom(e);
        }

        private void SocketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
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

        void SendKey(string inValue)
        {

            //SocketAsyncEventArgs sendSocketEventArg = new SocketAsyncEventArgs();
            byte[] buffer = Encoding.UTF8.GetBytes("key " + inValue + "\n");
            remoteSocketEventArg.SetBuffer(buffer, 0, buffer.Length);

            remoteSocket.SendAsync(remoteSocketEventArg);
        }
        void SendJump(string inValue)
        {

            //SocketAsyncEventArgs sendSocketEventArg = new SocketAsyncEventArgs();
            byte[] buffer = Encoding.UTF8.GetBytes("jump " + inValue + "\n");
            remoteSocketEventArg.SetBuffer(buffer, 0, buffer.Length);

            remoteSocket.SendAsync(remoteSocketEventArg);
        }
        void SendQuery(string inValue)
        {

            //SocketAsyncEventArgs sendSocketEventArg = new SocketAsyncEventArgs();
            byte[] buffer = Encoding.UTF8.GetBytes("query " + inValue + "\n");
            remoteSocketEventArg.SetBuffer(buffer, 0, buffer.Length);

            remoteSocket.SendAsync(remoteSocketEventArg);
        }

        void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Connected to the server successfully
                //remoteSocket.ReceiveAsync(e);
                //Read data sent from the server
                //Socket sock = e.UserToken as Socket;

                /*
                bool willRaiseEvent = remoteSocket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
                */

                byte[] buffer = Encoding.UTF8.GetBytes("\n");
                remoteSocketEventArg.SetBuffer(buffer, 0, buffer.Length);

                remoteSocket.SendAsync(remoteSocketEventArg);
                
            }
            else
            {
                //throw new SocketException((int)e.SocketError);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error connecting to frontend.  Check your settings.");
                });
            }
        }
        void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Sent "Hello World" to the server successfully

                //Read data sent from the server
                //Socket sock = e.UserToken as Socket;

                e.SetBuffer(new Byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);
                bool willRaiseEvent = remoteSocket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                //throw new SocketException((int)e.SocketError);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error connecting to frontend.  Check your settings.");
                });
            }
        }
        protected void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Received data from server
                string newReponse = Encoding.UTF8.GetString(e.Buffer, 0, e.Buffer.Length);

                if (newReponse == "")
                {
                    //done getting response
                }
                else
                {

                    responseText = Encoding.UTF8.GetString(e.Buffer, 0, e.Buffer.Length);
                    /*
                    bool willRaiseEvent = remoteSocket.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(e);

                    }
                     */ 
                }
            }
            else
            {
                //throw new SocketException((int)e.SocketError);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error connecting to frontend.  Check your settings.");
                });
            }
        }

        //buttons
        //navigation
        private void escape_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("escape");
        }
        private void up_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("up");
        }
        private void delete_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("d");
        }
        private void left_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("left");
        }
        private void enter_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("enter");
        }
        private void right_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("right");
        }
        private void menu_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("m");
        }
        private void down_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("down");
        }
        private void info_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("i");
        }

        //playback
        private void previous_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("q");
        }
        private void pause_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("p");
        }
        private void next_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendKey("z");
        }
        private void livetv_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendJump("livetv");
        }
        private void recorded_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendJump("playbackbox");
        }
        private void video_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendJump("mythvideo");
        }
        private void music_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendJump("playmusic");
        }

        //query
        private void location_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendQuery("location");
        }
        private void volume_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SendQuery("volume");
        }

        private void keyboardBox_KeyUp(object sender, KeyEventArgs e)
        {
            //MessageBox.Show("entered key: " + e.Key);
            string newKey = e.Key.ToString();

            switch(e.Key.ToString())
            {
                case "D1":
                    newKey = "1";
                    break;
                case "D2":
                    newKey = "2";
                    break;
                case "D3":
                    newKey = "3";
                    break;
                case "D4":
                    newKey = "4";
                    break;
                case "D5":
                    newKey = "5";
                    break;
                case "D6":
                    newKey = "6";
                    break;
                case "D7":
                    newKey = "7";
                    break;
                case "D8":
                    newKey = "8";
                    break;
                case "D9":
                    newKey = "9";
                    break;
                case "D0":
                    newKey = "0";
                    break;
                case "Back":
                    newKey = "escape";
                    break;

            }

            SendKey(newKey);

            keyboardBox.Text = "";
        }

        private void remotePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (remotePivot.SelectedIndex)
            {
                case 0:
                    //navigation
                    keyboardBox.IsEnabled = false;
                    break;
                case 1:
                    //playback
                    keyboardBox.IsEnabled = false;
                    break;
                case 2:
                    keyboardBox.IsEnabled = true;
                    keyboardBox.Focus();
                    break;
                case 3:
                    //query
                    keyboardBox.IsEnabled = false;
                    break;
            }
        }

    }
}