﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

namespace MythMe
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            GotSettings = false;

            CurrentBackend = new BackendViewModel();
            CurrentBackendIndex = 0;

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private string getSettingsString = "http://{0}:{1}/Myth/GetSetting?random={2}";
        private string getHosts25String = "http://{0}:{1}/Myth/GetHosts?random={2}";
        private string getHostsString = "http://{0}:{1}/Myth/GetHosts?random={2}";
        private string getSettingsHostNameString = "http://{0}:{1}/Myth/GetSetting?HostName={2}&random={3}";

        private bool GotSettings;

        private BackendViewModel CurrentBackend;
        private int CurrentBackendIndex;

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();

                if ((App.ViewModel.appSettings.FirstRunSetting) || (false))
                {
                    MessageBox.Show("Welcome to MythMe.  This is an app for controlling a MythTV DVR system.  If you do not know what that means this app is not for you.  You will need to enter your master backend address in the preferences to get started.  This app does not yet support the 0.25-development branch of MythTV.", "MythMe", MessageBoxButton.OK);

                    App.ViewModel.appSettings.FirstRunSetting = false;

                    NavigationService.Navigate(new Uri("/Preferences.xaml", UriKind.Relative));
                }

            }

            //App.ViewModel.appSettings.RemoteIndexSetting = 2;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            MasterBackendTitle.Text = App.ViewModel.appSettings.MasterBackendIpSetting;

            if ((!GotSettings)&&(App.ViewModel.appSettings.MasterBackendIpSetting.Length > 0))
            {
                performanceProgressBarCustomized.IsIndeterminate = true;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getSettingsString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(SettingsCallback), webRequest);
            }

        }

        private void SettingsCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get settings data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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

                foreach (XElement singleValueElement in xdoc.Element("GetSettingResponse").Element("Values").Descendants("Value"))
                {

                    switch (singleValueElement.Attribute("key").Value)
                    {
                        case "DBSchemaVer":
                            App.ViewModel.appSettings.DBSchemaVerSetting = int.Parse(singleValueElement.FirstNode.ToString());
                            break;
                        case "MasterServerIP":
                            App.ViewModel.appSettings.MasterBackendIpSettingSetting = singleValueElement.FirstNode.ToString();
                            break;
                        case "MasterServerPort":
                            App.ViewModel.appSettings.MasterBackendPortSetting = int.Parse(singleValueElement.FirstNode.ToString());
                            break;

                        case "UserJobDesc1":
                            App.ViewModel.appSettings.UserJobDesc1Setting = singleValueElement.FirstNode.ToString();
                            break;
                        case "UserJobDesc2":
                            App.ViewModel.appSettings.UserJobDesc2Setting = singleValueElement.FirstNode.ToString();
                            break;
                        case "UserJobDesc3":
                            App.ViewModel.appSettings.UserJobDesc3Setting = singleValueElement.FirstNode.ToString();
                            break;
                        case "UserJobDesc4":
                            App.ViewModel.appSettings.UserJobDesc4Setting = singleValueElement.FirstNode.ToString();
                            break;

                        case "AutoCommercialFlag":
                            App.ViewModel.appSettings.AutoCommercialFlagSetting = App.ViewModel.functions.IntToBool(singleValueElement.FirstNode.ToString());
                            break;
                        case "AutoTranscode":
                            App.ViewModel.appSettings.AutoTranscodeSetting = App.ViewModel.functions.IntToBool(singleValueElement.FirstNode.ToString());
                            break;
                        case "AutoRunUserJob1":
                            App.ViewModel.appSettings.AutoRunUserJob1Setting = App.ViewModel.functions.IntToBool(singleValueElement.FirstNode.ToString());
                            break;
                        case "AutoRunUserJob2":
                            App.ViewModel.appSettings.AutoRunUserJob2Setting = App.ViewModel.functions.IntToBool(singleValueElement.FirstNode.ToString());
                            break;
                        case "AutoRunUserJob3":
                            App.ViewModel.appSettings.AutoRunUserJob3Setting = App.ViewModel.functions.IntToBool(singleValueElement.FirstNode.ToString());
                            break;
                        case "AutoRunUserJob4":
                            App.ViewModel.appSettings.AutoRunUserJob4Setting = App.ViewModel.functions.IntToBool(singleValueElement.FirstNode.ToString());
                            break;

                        case "MythXML_key":
                            App.ViewModel.appSettings.MythwebXmlKeySetting = singleValueElement.FirstNode.ToString();
                            break;

                        default:
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                //MessageBox.Show("Value key: " + singleValueElement.Attribute("key").Value);
                            });
                            break;
                    }


                }

            }
            catch (Exception ex)
            {

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error parsing setting xml: " + ex.ToString(), "data", MessageBoxButton.OK);
                });
            }

            GotSettings = true;

            GetHosts();
        }

        private void GetHosts()
        {

            if ((App.ViewModel.Backends.Count == 0) || (false))
            {
                App.ViewModel.Backends.Clear();

                if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
                {
                    //MessageBox.Show("about to get hosts");
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getHosts25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                    webRequest.BeginGetResponse(new AsyncCallback(Hosts25Callback), webRequest);
                }
                else
                {
                    //MessageBox.Show("about to get hosts");
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getHostsString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                    webRequest.BeginGetResponse(new AsyncCallback(HostsCallback), webRequest);
                }
            }
            else
            {

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    performanceProgressBarCustomized.IsIndeterminate = false;
                });

            }
        }
        
        private void Hosts25Callback(IAsyncResult asynchronousResult)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { MessageBox.Show("not yet supported"); });
        }

        private void HostsCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get hosts data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                });

                return;
            }

            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                resultString = streamReader1.ReadToEnd();
            }



            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                //MessageBox.Show("Got hosts data: " + resultString, "data", MessageBoxButton.OK);
            });

            response.GetResponseStream().Close();
            response.Close();

            try
            {

                XDocument xdoc = XDocument.Parse(resultString, LoadOptions.None);

                foreach (XElement singleHostElement in xdoc.Element("GetHostsResponse").Element("Hosts").Descendants("Host"))
                {

                    BackendViewModel newHost = new BackendViewModel() { Name = singleHostElement.FirstNode.ToString(), ProtoPort = 6543, XmlPort = 6544 };

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                         App.ViewModel.Backends.Add(newHost);

                         //MessageBox.Show("just added new backend: "+newHost.Name);
                    });


                }

            }
            catch (Exception ex)
            {

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("error parsing xml: " + ex.ToString(), "data", MessageBoxButton.OK);
                });
            }

            App.ViewModel.saveBackends();

            
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (App.ViewModel.Backends.Count > 0)
                {
                    CurrentBackendIndex = 0;
                    CurrentBackend = App.ViewModel.Backends[CurrentBackendIndex];

                    GetHostNameSettings();
                }
                else
                {
                
                    MessageBox.Show("Did not find any hosts.  Try closing and reopening the app.");
                    performanceProgressBarCustomized.IsIndeterminate = false;
                
                }
            });

        }

        private void GetHostNameSettings()
        {

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                //MessageBox.Show("About to get settings for hostname: "+CurrentBackend.Name);
            });

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getSettingsHostNameString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, CurrentBackend.Name, App.ViewModel.randText())));
            webRequest.BeginGetResponse(new AsyncCallback(SettingsHostNameCallback), webRequest);
        }

        private void SettingsHostNameCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get settings data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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
                
                CurrentBackend.Master = false;

                foreach (XElement singleValueElement in xdoc.Element("GetSettingResponse").Element("Values").Descendants("Value"))
                {
                    
                    switch (singleValueElement.Attribute("key").Value)
                    {
                        case "BackendServerIP":
                            if (singleValueElement.FirstNode.ToString() == App.ViewModel.appSettings.MasterBackendIpSettingSetting)
                            {
                                CurrentBackend.Master = true;
                                CurrentBackend.Address = App.ViewModel.appSettings.MasterBackendIpSetting;
                            }
                            else
                            {
                                CurrentBackend.Master = false;
                                CurrentBackend.Address = singleValueElement.FirstNode.ToString();
                            }
                            break;
                        case "BackendServerPort":
                            CurrentBackend.ProtoPort = int.Parse(singleValueElement.FirstNode.ToString());
                            break;
                        case "BackendStatusPort":
                            CurrentBackend.XmlPort = int.Parse(singleValueElement.FirstNode.ToString());
                            break;

                        case "NetworkControlEnabled":
                            CurrentBackend.NetworkControlEnabled = App.ViewModel.functions.IntToBool(singleValueElement.FirstNode.ToString());
                            break;
                        case "NetworkControlPort":
                            CurrentBackend.NetworkControlPort = int.Parse(singleValueElement.FirstNode.ToString());
                            break;

                        default:
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                //MessageBox.Show("Value key: " + singleValueElement.Attribute("key").Value);
                            });
                            break;
                    }


                }

            }
            catch (Exception ex)
            {

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error parsing setting xml: " + ex.ToString(), "data", MessageBoxButton.OK);
                });
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                App.ViewModel.Backends[CurrentBackendIndex] = CurrentBackend;

                CurrentBackendIndex++;

                if(CurrentBackendIndex == App.ViewModel.Backends.Count)
                {
                    App.ViewModel.functions.FrontendsFromBackends();

                    App.ViewModel.saveBackends();

                    performanceProgressBarCustomized.IsIndeterminate = false;
                    //MessageBox.Show("finished getting settings for "+App.ViewModel.Backends.Count+" hosts");

                }
                else
                {
                    CurrentBackend = App.ViewModel.Backends[CurrentBackendIndex];
                    GetHostNameSettings();
                }
            });

        }



        private void remoteButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Remote.xaml", UriKind.Relative));
        }

        private void recordedButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Recorded.xaml", UriKind.Relative));
        }

        private void upcomingButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Upcoming.xaml", UriKind.Relative));
        }

        private void guideButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Guide.xaml?SelectedNow=true", UriKind.Relative));
        }

        private void searchButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");
        }

        private void videosButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");
        }

        private void musicButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");
        }

        private void statusButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Status.xaml", UriKind.Relative));
        }

        private void logButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Not yet implimented");
        }

        private void preferencesButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Preferences.xaml", UriKind.Relative));
        }

        private void helpButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Help.xaml", UriKind.Relative));
        }

        private void reload_Click(object sender, EventArgs e)
        {

            performanceProgressBarCustomized.IsIndeterminate = true;

            App.ViewModel.Backends.Clear();

            GetHosts();
        }
    }
}