using System;
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
using Microsoft.Phone.Tasks;

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

        private List<NameContentViewModel> menuListItems;

        private string getSettingsString = "http://{0}:{1}/Myth/GetSetting?random={2}";
        private string getHosts25String = "http://{0}:{1}/Myth/GetHosts?random={2}";
        private string getHostsString = "http://{0}:{1}/Myth/GetHosts?random={2}";
        private string getSettings25HostNameString = "http://{0}:{1}/Myth/GetSetting?HostName={2}&random={3}";
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

                App.ViewModel.appSettings.AppStartsSetting = App.ViewModel.appSettings.AppStartsSetting + 1;

                if ((App.ViewModel.appSettings.FirstRunSetting) || (false))
                {
                    MessageBox.Show("Welcome to MythMe.  This is an app for controlling a MythTV DVR system.  If you do not know what that means this app is not for you.  You will need to enter your master backend address in the preferences to get started. ", "MythMe", MessageBoxButton.OK);

                    App.ViewModel.appSettings.FirstRunSetting = false;

                    NavigationService.Navigate(new Uri("/Preferences.xaml", UriKind.Relative));
                }
                else if ((!App.ViewModel.appSettings.PromptScriptSetting)&&(!App.ViewModel.appSettings.UseScriptSetting) && (App.ViewModel.appSettings.AppStartsSetting > 4) && (App.ViewModel.appSettings.DBSchemaVerSetting <= 1269))
                {
                    App.ViewModel.appSettings.PromptScriptSetting = true;

                    if (MessageBox.Show("It is strongly recommended to use the optional script to unlock the full feature set of this app.  Using the script allows for scheduling recordings, queue jobs, searching for programs and viewing people details.  You can download the script from the app homepage.", "Script", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        WebBrowserTask webopen = new WebBrowserTask();

                        webopen.Uri = new Uri("http://code.google.com/p/mythme-wp7/downloads/detail?name=webmyth.py");
                        webopen.Show();
                    }
                }
                else if ((!App.ViewModel.appSettings.PromptScriptSetting) && (App.ViewModel.appSettings.UseScriptSetting) && (App.ViewModel.appSettings.AppStartsSetting > 0))
                {
                    App.ViewModel.appSettings.PromptScriptSetting = true;

                    if (MessageBox.Show("If you had previously downloaded the webmyth.py script before January 4th, you will need to upgrade to the latest version (11) for this release of the MythMe app.  You can download the script from the app homepage.", "Script", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        WebBrowserTask webopen = new WebBrowserTask();

                        webopen.Uri = new Uri("http://code.google.com/p/mythme-wp7/downloads/detail?name=webmyth.py");
                        webopen.Show();
                    }
                }
                else if ((!App.ViewModel.appSettings.ReviewedSetting) && (App.ViewModel.appSettings.AppStartsSetting > 9))
                {
                    App.ViewModel.appSettings.ReviewedSetting = true;

                    if (MessageBox.Show("Would you mind taking a minute to review this app in the marketplace?", "App Review", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        MarketplaceDetailTask marketDetail = new MarketplaceDetailTask();
                        marketDetail.ContentIdentifier = "455f5645-0b06-429b-9cac-9097b10ae6d2";
                        marketDetail.Show();
                    }
                }
                    

            }

            //App.ViewModel.appSettings.RemoteIndexSetting = 2;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            MasterBackendTitle.Text = App.ViewModel.appSettings.MasterBackendIpSetting;


            this.UpdateMenu();


            if ((!GotSettings)&&(App.ViewModel.appSettings.MasterBackendIpSetting.Length > 0))
            {
                performanceProgressBarCustomized.IsIndeterminate = true;

                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.1.105/dropbox/GetSetting.xml"));
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getSettingsString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(SettingsCallback), webRequest);
            }

            if ("" == App.ViewModel.appSettings.WebserverHostSetting)
            {
                App.ViewModel.appSettings.WebserverHostSetting = ""+App.ViewModel.appSettings.MasterBackendIpSetting;      
            }

            if ((App.ViewModel.appSettings.UseScriptScreenshotsSetting) && (!App.ViewModel.appSettings.UseScriptSetting))
            {
                App.ViewModel.appSettings.UseScriptScreenshotsSetting = false;
            }

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                //App.ViewModel.appSettings.UseScriptSetting = false;
            }
            else
            {
                App.ViewModel.appSettings.UseServicesUpcomingSetting = false;
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

                if (xdoc.Descendants("GetSettingResponse").Count() > 0)
                {

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
                            case "AutoMetadataLookup":
                                App.ViewModel.appSettings.AutoMetadataLookupSetting = App.ViewModel.functions.IntToBool(singleValueElement.FirstNode.ToString());
                                break;

                            case "DefaultStartOffset":
                                App.ViewModel.appSettings.DefaultStartOffsetSetting = int.Parse(singleValueElement.FirstNode.ToString());
                                break;
                            case "DefaultEndOffset":
                                App.ViewModel.appSettings.DefaultEndOffsetSetting = int.Parse(singleValueElement.FirstNode.ToString());
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
                else if(xdoc.Descendants("SettingList").Count() > 0)
                {

                    foreach (XElement singleValueElement in xdoc.Element("SettingList").Element("Settings").Descendants("String"))
                    {

                        switch (singleValueElement.Element("Key").FirstNode.ToString())
                        {
                            case "DBSchemaVer":
                                App.ViewModel.appSettings.DBSchemaVerSetting = int.Parse(singleValueElement.Element("Value").FirstNode.ToString());
                                break;
                            case "MasterServerIP":
                                App.ViewModel.appSettings.MasterBackendIpSettingSetting = singleValueElement.Element("Value").FirstNode.ToString();
                                break;
                            case "MasterServerPort":
                                App.ViewModel.appSettings.MasterBackendPortSetting = int.Parse(singleValueElement.Element("Value").FirstNode.ToString());
                                break;

                            case "UserJobDesc1":
                                App.ViewModel.appSettings.UserJobDesc1Setting = singleValueElement.Element("Value").FirstNode.ToString();
                                break;
                            case "UserJobDesc2":
                                App.ViewModel.appSettings.UserJobDesc2Setting = singleValueElement.Element("Value").FirstNode.ToString();
                                break;
                            case "UserJobDesc3":
                                App.ViewModel.appSettings.UserJobDesc3Setting = singleValueElement.Element("Value").FirstNode.ToString();
                                break;
                            case "UserJobDesc4":
                                App.ViewModel.appSettings.UserJobDesc4Setting = singleValueElement.Element("Value").FirstNode.ToString();
                                break;

                            case "AutoCommercialFlag":
                                App.ViewModel.appSettings.AutoCommercialFlagSetting = App.ViewModel.functions.IntToBool(singleValueElement.Element("Value").FirstNode.ToString());
                                break;
                            case "AutoTranscode":
                                App.ViewModel.appSettings.AutoTranscodeSetting = App.ViewModel.functions.IntToBool(singleValueElement.Element("Value").FirstNode.ToString());
                                break;
                            case "AutoRunUserJob1":
                                App.ViewModel.appSettings.AutoRunUserJob1Setting = App.ViewModel.functions.IntToBool(singleValueElement.Element("Value").FirstNode.ToString());
                                break;
                            case "AutoRunUserJob2":
                                App.ViewModel.appSettings.AutoRunUserJob2Setting = App.ViewModel.functions.IntToBool(singleValueElement.Element("Value").FirstNode.ToString());
                                break;
                            case "AutoRunUserJob3":
                                App.ViewModel.appSettings.AutoRunUserJob3Setting = App.ViewModel.functions.IntToBool(singleValueElement.Element("Value").FirstNode.ToString());
                                break;
                            case "AutoRunUserJob4":
                                App.ViewModel.appSettings.AutoRunUserJob4Setting = App.ViewModel.functions.IntToBool(singleValueElement.Element("Value").FirstNode.ToString());
                                break;
                            case "AutoMetadataLookup":
                                App.ViewModel.appSettings.AutoMetadataLookupSetting = App.ViewModel.functions.IntToBool(singleValueElement.Element("Value").FirstNode.ToString());
                                break;

                            case "DefaultStartOffset":
                                App.ViewModel.appSettings.DefaultStartOffsetSetting = int.Parse(singleValueElement.Element("Value").FirstNode.ToString());
                                break;
                            case "DefaultEndOffset":
                                App.ViewModel.appSettings.DefaultEndOffsetSetting = int.Parse(singleValueElement.Element("Value").FirstNode.ToString());
                                break;

                            case "MythXML_key":
                                App.ViewModel.appSettings.MythwebXmlKeySetting = singleValueElement.Element("Value").FirstNode.ToString();
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

            if (((App.ViewModel.Backends.Count == 0)&&(App.ViewModel.appSettings.MasterBackendIpSetting.Length > 0)) || (false))
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

                    this.UpdateMenu();
                });

            }
        }
        
        private void Hosts25Callback(IAsyncResult asynchronousResult)
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

                foreach (XElement singleHostElement in xdoc.Element("StringList").Descendants("String"))
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
                    MessageBox.Show("Error parsing hosts xml: " + ex.ToString(), "data", MessageBoxButton.OK);
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


            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                //MessageBox.Show("about to get hosts");
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getSettings25HostNameString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, CurrentBackend.Name, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(Settings25HostNameCallback), webRequest);
            }
            else
            {
                //MessageBox.Show("about to get hosts");
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getSettingsHostNameString, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, CurrentBackend.Name, App.ViewModel.randText())));
                webRequest.BeginGetResponse(new AsyncCallback(SettingsHostNameCallback), webRequest);
            }

        }

        private void Settings25HostNameCallback(IAsyncResult asynchronousResult)
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

                foreach (XElement singleValueElement in xdoc.Element("SettingList").Element("Settings").Descendants("String"))
                {
                    
                    switch (singleValueElement.Element("Key").Value)
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
                                CurrentBackend.Address = singleValueElement.Element("Value").FirstNode.ToString();
                            }
                            break;
                        case "BackendServerPort":
                            CurrentBackend.ProtoPort = int.Parse(singleValueElement.Element("Value").FirstNode.ToString());
                            break;
                        case "BackendStatusPort":
                            CurrentBackend.XmlPort = int.Parse(singleValueElement.Element("Value").FirstNode.ToString());
                            break;

                        case "NetworkControlEnabled":
                            CurrentBackend.NetworkControlEnabled = App.ViewModel.functions.IntToBool(singleValueElement.Element("Value").FirstNode.ToString());
                            break;
                        case "NetworkControlPort":
                            CurrentBackend.NetworkControlPort = int.Parse(singleValueElement.Element("Value").FirstNode.ToString());
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

                    this.UpdateMenu();

                }
                else
                {
                    CurrentBackend = App.ViewModel.Backends[CurrentBackendIndex];
                    GetHostNameSettings();
                }
            });

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

                if (CurrentBackendIndex == App.ViewModel.Backends.Count)
                {
                    App.ViewModel.functions.FrontendsFromBackends();

                    App.ViewModel.saveBackends();

                    performanceProgressBarCustomized.IsIndeterminate = false;
                    //MessageBox.Show("finished getting settings for "+App.ViewModel.Backends.Count+" hosts");

                    this.UpdateMenu();

                }
                else
                {
                    CurrentBackend = App.ViewModel.Backends[CurrentBackendIndex];
                    GetHostNameSettings();
                }
            });

        }



        private void reload_Click(object sender, EventArgs e)
        {

            performanceProgressBarCustomized.IsIndeterminate = true;

            App.ViewModel.Backends.Clear();

            if (App.ViewModel.appSettings.MasterBackendIpSetting.Length > 0)
            {
                GetHosts();
            }
            else
            {
                MessageBox.Show("You must setup a master backend address before reloading the backends and settings.");

                performanceProgressBarCustomized.IsIndeterminate = false;
            }
        }

        private void menuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (menuList.SelectedItem == null)
                return;

            var thisItem = (NameContentViewModel)menuList.SelectedItem;

            switch (thisItem.Content)
            {
                case "remote":
                    NavigationService.Navigate(new Uri("/Remote.xaml", UriKind.Relative));
                    break;
                case "recorded":
                    NavigationService.Navigate(new Uri("/Recorded.xaml", UriKind.Relative));
                    break;
                case "upcoming":
                    NavigationService.Navigate(new Uri("/Upcoming.xaml", UriKind.Relative));
                    break;
                case "guide":
                    NavigationService.Navigate(new Uri("/Guide.xaml?SelectedNow=true", UriKind.Relative));
                    break;
                case "search":
                    NavigationService.Navigate(new Uri("/Search.xaml", UriKind.Relative));
                    break;
                case "videos":
                    NavigationService.Navigate(new Uri("/Videos.xaml", UriKind.Relative));
                    break;
                case "music":
                    //NavigationService.Navigate(new Uri("/Music.xaml", UriKind.Relative));
                    break;
                case "people":
                    NavigationService.Navigate(new Uri("/People.xaml", UriKind.Relative));
                    break;
                case "status":
                    NavigationService.Navigate(new Uri("/Status.xaml", UriKind.Relative));
                    break;
                case "log":
                    NavigationService.Navigate(new Uri("/Log.xaml", UriKind.Relative));
                    break;
                case "downloads":
                    NavigationService.Navigate(new Uri("/Downloads.xaml", UriKind.Relative));
                    break;
                case "stream":
                    NavigationService.Navigate(new Uri("/Stream.xaml", UriKind.Relative));
                    break;
                case "preferences":
                    NavigationService.Navigate(new Uri("/Preferences.xaml", UriKind.Relative));
                    break;
                case "help":
                    NavigationService.Navigate(new Uri("/Help.xaml", UriKind.Relative));
                    break;
            }

        }

        private void preferencesButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Preferences.xaml", UriKind.Relative));
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Help.xaml", UriKind.Relative));
        }


        private void UpdateMenu()
        {

            menuListItems = new List<NameContentViewModel>();

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 0)
            {
                menuListItems.Add(new NameContentViewModel() { Content = "remote" });
                menuListItems.Add(new NameContentViewModel() { Content = "recorded" });
                menuListItems.Add(new NameContentViewModel() { Content = "upcoming" });
                menuListItems.Add(new NameContentViewModel() { Content = "guide" });
                //menuListItems.Add(new NameContentViewModel() {Content = "music"});

                if ((App.ViewModel.appSettings.UseScriptSetting) || (App.ViewModel.appSettings.DBSchemaVerSetting > 1269))
                {
                    menuListItems.Add(new NameContentViewModel() { Content = "videos" });
                }

                if (App.ViewModel.appSettings.UseScriptSetting)
                {
                    menuListItems.Add(new NameContentViewModel() { Content = "search" });
                    menuListItems.Add(new NameContentViewModel() { Content = "people" });
                }

                menuListItems.Add(new NameContentViewModel() { Content = "status" });

                if ((App.ViewModel.appSettings.UseScriptSetting) && (App.ViewModel.appSettings.DBSchemaVerSetting <= 1269))
                {
                    menuListItems.Add(new NameContentViewModel() { Content = "log" });
                }

                if (App.ViewModel.appSettings.AllowDownloadsSetting)
                {
                    menuListItems.Add(new NameContentViewModel() { Content = "downloads" });
                }

                if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
                {
                    //stream format from mythtv doesnt work on WP7
                    //menuListItems.Add(new NameContentViewModel() { Content = "stream" });
                }

                //menuListItems.Add(new NameContentViewModel() { Content = "preferences" });
                //menuListItems.Add(new NameContentViewModel() { Content = "help" });
            }
            else
            {
                menuListItems.Add(new NameContentViewModel() { Content = "preferences" });
            }


            menuList.ItemsSource = menuListItems;

        }



    }
}