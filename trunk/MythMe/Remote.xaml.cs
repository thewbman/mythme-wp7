using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
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
            remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            remoteSocketEventArg = new SocketAsyncEventArgs();
            remoteAddress = "192.168.1.104";
            remotePort = 6546; 
            remoteEndPoint = new DnsEndPoint(remoteAddress, remotePort);

            this.responseText = "";
            
        }

        private bool connected;
        private Socket remoteSocket;
        SocketAsyncEventArgs remoteSocketEventArg;
        private string remoteAddress;
        private int remotePort;
        private DnsEndPoint remoteEndPoint;

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

            remoteTitle.Title = "remote: " + remoteAddress;

            if(connected == false) {
               remoteSocketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Completed);
                
               remoteSocketEventArg.RemoteEndPoint = remoteEndPoint;
               remoteSocketEventArg.UserToken = remoteSocket;

               remoteSocket.ConnectAsync(remoteSocketEventArg);

               connected = true;
               
            }
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
            }
            else
            {
                throw new SocketException((int)e.SocketError);
            }
        }
        void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Sent "Hello World" to the server successfully

                //Read data sent from the server
                //Socket sock = e.UserToken as Socket;
                bool willRaiseEvent = remoteSocket.ReceiveAsync(e);
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
                throw new SocketException((int)e.SocketError);
            }
        }

        //buttons
        //navigation
        private void escape_Tap(object sender, GestureEventArgs e)
        {
            SendKey("escape");
        }
        private void up_Tap(object sender, GestureEventArgs e)
        {
            SendKey("up");
        }
        private void delete_Tap(object sender, GestureEventArgs e)
        {
            SendKey("d");
        }
        private void left_Tap(object sender, GestureEventArgs e)
        {
            SendKey("left");
        }
        private void enter_Tap(object sender, GestureEventArgs e)
        {
            SendKey("enter");
        }
        private void right_Tap(object sender, GestureEventArgs e)
        {
            SendKey("right");
        }
        private void menu_Tap(object sender, GestureEventArgs e)
        {
            SendKey("m");
        }
        private void down_Tap(object sender, GestureEventArgs e)
        {
            SendKey("down");
        }
        private void info_Tap(object sender, GestureEventArgs e)
        {
            SendKey("i");
        }

        //playback
        private void previous_Tap(object sender, GestureEventArgs e)
        {
            SendKey("q");
        }
        private void pause_Tap(object sender, GestureEventArgs e)
        {
            SendKey("p");
        }
        private void next_Tap(object sender, GestureEventArgs e)
        {
            SendKey("z");
        }
        private void livetv_Tap(object sender, GestureEventArgs e)
        {
            SendJump("livetv");
        }
        private void recorded_Tap(object sender, GestureEventArgs e)
        {
            SendJump("playbackbox");
        }
        private void video_Tap(object sender, GestureEventArgs e)
        {
            SendJump("mythvideo");
        }
        private void music_Tap(object sender, GestureEventArgs e)
        {
            SendJump("playmusic");
        }

        //query
        private void location_Tap(object sender, GestureEventArgs e)
        {
            SendQuery("location");
        }
        private void volume_Tap(object sender, GestureEventArgs e)
        {
            SendQuery("volume");
        }

    }
}