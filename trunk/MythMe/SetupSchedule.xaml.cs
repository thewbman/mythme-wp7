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
using Microsoft.Phone.Controls.Primitives;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Runtime.Serialization.Json;
using Microsoft.Phone.Shell;

namespace MythMe
{
    public partial class SetupSchedule : PhoneApplicationPage
    {
        public SetupSchedule()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedSetupProgram;

            RuleTypes = new List<NameContentViewModel>();
            Inputs = new List<NameContentViewModel>();

            CurrentRule = new RuleViewModel();

            SchedulerRule = -1;

            HasLoaded = false;
        }

        private List<NameContentViewModel> RuleTypes;
        private List<NameContentViewModel> Inputs;

        private RuleViewModel CurrentRule;
        private RuleViewModel NewRule;

        private int SchedulerRule;

        private string getRule25String = "http://{0}:{1}/Dvr/GetRecordSchedule?RecordId={2}&random={3}";
        private string addRule25String = "http://{0}:{1}/Dvr/AddRecordSchedule";  //POST
        private string removeRule25String = "http://{0}:{1}/Dvr/RemoveRecordSchedule";  //POST
        private string disableRule25String = "http://{0}:{1}/Dvr/DisableRecordSchedule";  //POST
        private string enableRule25String = "http://{0}:{1}/Dvr/EnableRecordSchedule";  //POST


        private bool HasLoaded;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            /*
             * 
			{caption: $L("Record anytime"), value: 4},
			{caption: $L("Anytime on channel"), value: 3},
			{caption: $L("Find one each week"), value: 10},
			{caption: $L("Find one each day"), value: 9},
			{caption: $L("Find one showing"), value: 6},
			{caption: $L("Timeslot every week"), value: 5},
			{caption: $L("Timeslot every day"), value: 2},
			{caption: $L("Only this showing"), value: 1},
			{caption: $L("No recording rule"), value: 0},
             * 
             * 
			{caption: $L("Force record"), value: 7},
			{caption: $L("Force don't record"), value: 8},
             */

            autouserjob1.Content = App.ViewModel.appSettings.UserJobDesc1Setting;
            autouserjob2.Content = App.ViewModel.appSettings.UserJobDesc2Setting;
            autouserjob3.Content = App.ViewModel.appSettings.UserJobDesc3Setting;
            autouserjob4.Content = App.ViewModel.appSettings.UserJobDesc4Setting;


            ApplicationBar = new ApplicationBar();

            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 1.0; 
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.BackgroundColor = Color.FromArgb(255,40,0,12);
            ApplicationBar.ForegroundColor = Colors.White;

            ApplicationBarIconButton appbarBack = new ApplicationBarIconButton();
            appbarBack.IconUri = new Uri("/Images/appbar.back.rest.png", UriKind.Relative);
            appbarBack.Text = "go back";
            appbarBack.Click += new EventHandler(appbarBack_Click);
            ApplicationBar.Buttons.Add(appbarBack);

            ApplicationBarIconButton appbarSave = new ApplicationBarIconButton();
            appbarSave.IconUri = new Uri("/Images/appbar.save.rest.png", UriKind.Relative);
            appbarSave.Text = "save";
            appbarSave.Click += new EventHandler(appbarSave_Click);
            ApplicationBar.Buttons.Add(appbarSave);

            ApplicationBarIconButton appbarDelete = new ApplicationBarIconButton();
            appbarDelete.IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative);
            appbarDelete.Text = "delete";
            appbarDelete.Click += new EventHandler(appbarDelete_Click);
            ApplicationBar.Buttons.Add(appbarDelete);

            ApplicationBarIconButton mythweb = new ApplicationBarIconButton();
            mythweb.IconUri = new Uri("/Images/website.png", UriKind.Relative);
            mythweb.Text = "mythweb";
            mythweb.Click += new EventHandler(mythweb_Click);
            ApplicationBar.Buttons.Add(mythweb);


            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                autometalookup.Visibility = System.Windows.Visibility.Visible;

                //0.25 Services API wont create new force record rules, it overwrites the parent rule


                ApplicationBarMenuItem enableRule = new ApplicationBarMenuItem();
                enableRule.Text = "enable rule";
                enableRule.Click += new EventHandler(enableRule_Click);
                ApplicationBar.MenuItems.Add(enableRule);

                ApplicationBarMenuItem disableRule = new ApplicationBarMenuItem();
                disableRule.Text = "disable rule";
                disableRule.Click += new EventHandler(disableRule_Click);
                ApplicationBar.MenuItems.Add(disableRule);
            }
            else
            {
                autometalookup.Visibility = System.Windows.Visibility.Collapsed;

                ApplicationBarMenuItem forceRecord = new ApplicationBarMenuItem();
                forceRecord.Text = "force record";
                forceRecord.Click += new EventHandler(forceRecord_Click);
                ApplicationBar.MenuItems.Add(forceRecord);

                ApplicationBarMenuItem forceDontRecord = new ApplicationBarMenuItem();
                forceDontRecord.Text = "force don't record";
                forceDontRecord.Click += new EventHandler(forceDontRecord_Click);
                ApplicationBar.MenuItems.Add(forceDontRecord);

                ApplicationBarMenuItem forgetOld = new ApplicationBarMenuItem();
                forgetOld.Text = "forget old";
                forgetOld.Click += new EventHandler(forgetOld_Click);
                ApplicationBar.MenuItems.Add(forgetOld);

                ApplicationBarMenuItem neverRecord = new ApplicationBarMenuItem();
                neverRecord.Text = "never record";
                neverRecord.Click += new EventHandler(neverRecord_Click);
                ApplicationBar.MenuItems.Add(neverRecord);
            }

            /*
            ApplicationBarMenuItem mythweb = new ApplicationBarMenuItem();
            mythweb.Text = "mythweb";
            mythweb.Click += new EventHandler(mythweb_Click);
            ApplicationBar.MenuItems.Add(mythweb);
             */


            if (!HasLoaded)
            {

                performanceProgressBarCustomized.IsIndeterminate = true;

                this.GetInputs();
                
                //this.GetRule();
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //HasLoaded = false;
            
            base.OnNavigatedFrom(e);
        }

        private void GetInputs()
        {
            try
            {
                if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
                {
                    //no method for getting inputs :(
                    
                    Inputs = new List<NameContentViewModel>();

                    Inputs.Add(new NameContentViewModel() { Name = "None", Content = "0" });

                    this.GetRule();
                }
                else
                {
                    string query = "SELECT cardinputid AS Content, displayname AS Name FROM cardinput ORDER BY Content; ";

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                    webRequest.BeginGetResponse(new AsyncCallback(InputsCallback), webRequest);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error requesting inputs data: " + ex.ToString());
            }
        }
        private void InputsCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get inputs data: " + ex.ToString(), "Error", MessageBoxButton.OK);

                    Inputs = new List<NameContentViewModel>();

                    Inputs.Add(new NameContentViewModel() { Name = "None", Content = "0" });

                    this.GetRule();

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

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<NameContentViewModel>));

                List<NameContentViewModel> newInputs = (List<NameContentViewModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got inputs: " + newInputs.Count);

                    Inputs = new List<NameContentViewModel>();

                    Inputs.Add(new NameContentViewModel(){Name = "None", Content = "0"});

                    foreach (var t in newInputs)
                    {
                        Inputs.Add(t);
                    }


                    this.GetRule();

                });

                //MessageBox.Show(Rules.ToString());

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error getting inputs: " + ex.ToString());

                    Inputs = new List<NameContentViewModel>();

                    Inputs.Add(new NameContentViewModel() { Name = "None", Content = "0" });

                    this.GetRule();
                });

            }

        }

        private void GetRule()
        {

            if (App.ViewModel.SelectedSetupProgram.recordid == 0)
            {
                this.DefaultRule();
            }
            else
            {

                try
                {

                    if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(getRule25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting, App.ViewModel.SelectedSetupProgram.recordid, App.ViewModel.randText())));
                        webRequest.BeginGetResponse(new AsyncCallback(Rule25Callback), webRequest);
                    }
                    else
                    {
                        string query = "SELECT * FROM record WHERE recordid=" + App.ViewModel.SelectedSetupProgram.recordid + " LIMIT 1; ";

                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQLwithResponse&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                        webRequest.BeginGetResponse(new AsyncCallback(RuleCallback), webRequest);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error requesting rule data: " + ex.ToString());
                }

            }
        }
        private void Rule25Callback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get record rule data: " + ex.ToString(), "Error", MessageBoxButton.OK);
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



            XDocument xdoc = XDocument.Parse(resultString, LoadOptions.None);

            if (xdoc.Element("detail") != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Invalid recording rule (#"+App.ViewModel.SelectedSetupProgram.recordid+") - using default", "Error", MessageBoxButton.OK);
                    this.DefaultRule();
                });
            }
            else
            {
                RuleViewModel recRule = new RuleViewModel();

                XElement recRuleElement = xdoc.Element("RecRule");

                if (recRuleElement.Element("Id").FirstNode != null) recRule.recordid = int.Parse(recRuleElement.Element("Id").FirstNode.ToString());
                if (recRuleElement.Element("ParentId").FirstNode != null) recRule.parentid = int.Parse(recRuleElement.Element("ParentId").FirstNode.ToString());
                if (recRuleElement.Element("Inactive").FirstNode != null) recRule.inactive = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("Inactive").FirstNode.ToString());

                if (recRuleElement.Element("Title").FirstNode != null) recRule.title = recRuleElement.Element("Title").FirstNode.ToString();
                if (recRuleElement.Element("SubTitle").FirstNode != null) recRule.subtitle = recRuleElement.Element("SubTitle").FirstNode.ToString();
                if (recRuleElement.Element("Description").FirstNode != null) recRule.description = recRuleElement.Element("Description").FirstNode.ToString();
                if (recRuleElement.Element("Season").FirstNode != null) recRule.season = recRuleElement.Element("Season").FirstNode.ToString();
                if (recRuleElement.Element("Episode").FirstNode != null) recRule.episode = recRuleElement.Element("Episode").FirstNode.ToString();
                if (recRuleElement.Element("Category").FirstNode != null) recRule.category = recRuleElement.Element("Category").FirstNode.ToString();

                if (recRuleElement.Element("StartTime").FirstNode != null) recRule.starttime = recRuleElement.Element("StartTime").FirstNode.ToString();
                if (recRuleElement.Element("EndTime").FirstNode != null) recRule.endtime = recRuleElement.Element("EndTime").FirstNode.ToString();
                if (recRuleElement.Element("SeriesId").FirstNode != null) recRule.seriesid = recRuleElement.Element("SeriesId").FirstNode.ToString();
                if (recRuleElement.Element("ProgramId").FirstNode != null) recRule.programid = recRuleElement.Element("ProgramId").FirstNode.ToString();
                if (recRuleElement.Element("Inetref").FirstNode != null) recRule.inetref = recRuleElement.Element("Inetref").FirstNode.ToString();

                if (recRuleElement.Element("ChanId").FirstNode != null) recRule.chanid = recRuleElement.Element("ChanId").FirstNode.ToString();
                if (recRuleElement.Element("CallSign").FirstNode != null) recRule.station = recRuleElement.Element("CallSign").FirstNode.ToString();
                if (recRuleElement.Element("Day").FirstNode != null) recRule.findday = int.Parse(recRuleElement.Element("Day").FirstNode.ToString());
                if (recRuleElement.Element("Time").FirstNode != null) recRule.findtime = recRuleElement.Element("Time").FirstNode.ToString();
                if (recRuleElement.Element("FindId").FirstNode != null) recRule.findid = int.Parse(recRuleElement.Element("FindId").FirstNode.ToString());
                if (recRuleElement.Element("Type").FirstNode != null) recRule.type = App.ViewModel.functions.ApiRecTypeToInt(recRuleElement.Element("Type").FirstNode.ToString(), App.ViewModel.SelectedSetupProgram.recstatus);
                if (recRuleElement.Element("SearchType").FirstNode != null) recRule.searchtype = recRuleElement.Element("SearchType").FirstNode.ToString();
                
                if (recRuleElement.Element("RecPriority").FirstNode != null) recRule.recpriority = int.Parse(recRuleElement.Element("RecPriority").FirstNode.ToString());
                if (recRuleElement.Element("PreferredInput").FirstNode != null) recRule.prefinput = int.Parse(recRuleElement.Element("PreferredInput").FirstNode.ToString());
                if (recRuleElement.Element("StartOffset").FirstNode != null) recRule.startoffset = int.Parse(recRuleElement.Element("StartOffset").FirstNode.ToString());
                if (recRuleElement.Element("EndOffset").FirstNode != null) recRule.endoffset = int.Parse(recRuleElement.Element("EndOffset").FirstNode.ToString());
                
                if (recRuleElement.Element("DupMethod").FirstNode != null) recRule.dupmethodtext = recRuleElement.Element("DupMethod").FirstNode.ToString();
                if (recRuleElement.Element("DupIn").FirstNode != null) recRule.dupintext = recRuleElement.Element("DupIn").FirstNode.ToString();
                if (recRuleElement.Element("Filter").FirstNode != null) recRule.filter = int.Parse(recRuleElement.Element("Filter").FirstNode.ToString());
                if (recRuleElement.Element("RecProfile").FirstNode != null) recRule.profile = recRuleElement.Element("RecProfile").FirstNode.ToString();
                if (recRuleElement.Element("RecGroup").FirstNode != null) recRule.recgroup = recRuleElement.Element("RecGroup").FirstNode.ToString();
                if (recRuleElement.Element("StorageGroup").FirstNode != null) recRule.storagegroup = recRuleElement.Element("StorageGroup").FirstNode.ToString();
                if (recRuleElement.Element("PlayGroup").FirstNode != null) recRule.playgroup = recRuleElement.Element("PlayGroup").FirstNode.ToString();

                if (recRuleElement.Element("AutoExpire").FirstNode != null) recRule.autoexpire = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("AutoExpire").FirstNode.ToString());
                if (recRuleElement.Element("MaxEpisodes").FirstNode != null) recRule.maxepisodes = int.Parse(recRuleElement.Element("MaxEpisodes").FirstNode.ToString());
                if (recRuleElement.Element("AutoCommflag").FirstNode != null) recRule.autocommflag = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("AutoCommflag").FirstNode.ToString());
                if (recRuleElement.Element("AutoTranscode").FirstNode != null) recRule.autotranscode = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("AutoTranscode").FirstNode.ToString());
                if (recRuleElement.Element("AutoMetaLookup").FirstNode != null) recRule.autometalookup = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("AutoMetaLookup").FirstNode.ToString());
                if (recRuleElement.Element("AutoUserJob1").FirstNode != null) recRule.autouserjob1 = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("AutoUserJob1").FirstNode.ToString());
                if (recRuleElement.Element("AutoUserJob2").FirstNode != null) recRule.autouserjob2 = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("AutoUserJob2").FirstNode.ToString());
                if (recRuleElement.Element("AutoUserJob3").FirstNode != null) recRule.autouserjob3 = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("AutoUserJob3").FirstNode.ToString());
                if (recRuleElement.Element("AutoUserJob4").FirstNode != null) recRule.autouserjob4 = App.ViewModel.functions.BoolTextToInt(recRuleElement.Element("AutoUserJob4").FirstNode.ToString());

                if (recRuleElement.Element("Transcoder").FirstNode != null) recRule.transcoder = int.Parse(recRuleElement.Element("Transcoder").FirstNode.ToString());
                

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {

                    CurrentRule = recRule;

                    this.Display();
                });
            }



            try
            {

            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Failed to parse recording rule: " + ex.ToString(), "Error", MessageBoxButton.OK);

                    this.DefaultRule();
                });

                return;
            }

        }
        private void RuleCallback(IAsyncResult asynchronousResult)
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
                    MessageBox.Show("Failed to get rule data: " + ex.ToString(), "Error", MessageBoxButton.OK);
                    this.DefaultRule();

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

                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<RuleViewModel>));

                List<RuleViewModel> Rules = (List<RuleViewModel>)s.ReadObject(response.GetResponseStream());


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Got rules: " + Rules.Count);

                    CurrentRule = Rules[0];

                    this.Display();

                });

                //MessageBox.Show(Rules.ToString());
                
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Error getting rule: " + ex.ToString());

                    this.DefaultRule();
                });

            }

        }


        private void DefaultRule()
        {
            CurrentRule = new RuleViewModel();

            CurrentRule.type = 0;
            CurrentRule.searchtype = "None";

            CurrentRule.prefinput = 0;
            CurrentRule.inactive = 0;
            CurrentRule.autoexpire = 1;
            CurrentRule.maxnewest = 0;
            CurrentRule.maxepisodes = 0;
            CurrentRule.startoffset = App.ViewModel.appSettings.DefaultStartOffsetSetting;
            CurrentRule.endoffset = App.ViewModel.appSettings.DefaultEndOffsetSetting;

            CurrentRule.autometalookup = App.ViewModel.functions.BoolToInt(App.ViewModel.appSettings.AutoMetadataLookupSetting);
            CurrentRule.autocommflag = App.ViewModel.functions.BoolToInt(App.ViewModel.appSettings.AutoCommercialFlagSetting);
            CurrentRule.autotranscode = App.ViewModel.functions.BoolToInt(App.ViewModel.appSettings.AutoTranscodeSetting);
            CurrentRule.autouserjob1 = App.ViewModel.functions.BoolToInt(App.ViewModel.appSettings.AutoRunUserJob1Setting);
            CurrentRule.autouserjob2 = App.ViewModel.functions.BoolToInt(App.ViewModel.appSettings.AutoRunUserJob2Setting);
            CurrentRule.autouserjob3 = App.ViewModel.functions.BoolToInt(App.ViewModel.appSettings.AutoRunUserJob3Setting);
            CurrentRule.autouserjob4 = App.ViewModel.functions.BoolToInt(App.ViewModel.appSettings.AutoRunUserJob4Setting);

            CurrentRule.profile = "Default";
            CurrentRule.recgroup = "Default";
            CurrentRule.dupmethod = 6;
            CurrentRule.dupin = 15;
            CurrentRule.search = 0;
            CurrentRule.parentid = 0;
            CurrentRule.transcoder = 0;
            CurrentRule.playgroup = "Default";
            CurrentRule.storagegroup = "Default";

            CurrentRule.filter = 0;

            CurrentRule.recordid = -1;

            this.Display();
        }

        private void Display()
        {
            RuleTypes = new List<NameContentViewModel>();

            if (CurrentRule.type == 0)
            {
                RuleTypes.Add(new NameContentViewModel(){Name = "Anytime on any channel", Content = "4"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Anytime on this channel", Content = "3"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one each week", Content = "10"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one each day", Content = "9"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one showing", Content = "6"});
                RuleTypes.Add(new NameContentViewModel(){Name = "This timeslot each week", Content = "5"});
                RuleTypes.Add(new NameContentViewModel(){Name = "This timeslot each day", Content = "2"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Only this showing", Content = "1"});
                RuleTypes.Add(new NameContentViewModel() { Name = "No recording rule", Content = "0" });

            }
            else if ((CurrentRule.type < 7)||(CurrentRule.type > 8))
            {
                RuleTypes.Add(new NameContentViewModel(){Name = "Anytime on any channel", Content = "4" });
                RuleTypes.Add(new NameContentViewModel(){Name = "Anytime on this channel", Content = "3"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one each week", Content = "10"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one each day", Content = "9"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one showing", Content = "6"});
                RuleTypes.Add(new NameContentViewModel(){Name = "This timeslot each week", Content = "5"});
                RuleTypes.Add(new NameContentViewModel(){Name = "This timeslot each day", Content = "2"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Only this showing", Content = "1"});
                //RuleTypes.Add(new NameContentViewModel(){Name = "No recording rule", Content = "0"});
            }
            else if(CurrentRule.type == 7)
            {
                RuleTypes.Add(new NameContentViewModel(){Name = "Force record", Content = "7"});
                //RuleTypes.Add(new NameContentViewModel(){Name = "Force don't record", Content = "8"});

            }
            else if(CurrentRule.type == 8)
            {
                //RuleTypes.Add(new NameContentViewModel(){Name = "Force record", Content = "7"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Force don't record", Content = "8"});

            }


            ruletype.ItemsSource = RuleTypes;

            switch (CurrentRule.type)
            {
                case 0:
                    ruletype.SelectedIndex = 8;
                    break;
                case 1:
                    ruletype.SelectedIndex = 7;
                    break;
                case 2:
                    ruletype.SelectedIndex = 6;
                    break;
                case 3:
                    ruletype.SelectedIndex = 1;
                    break;
                case 4:
                    ruletype.SelectedIndex = 0;
                    break;
                case 5:
                    ruletype.SelectedIndex = 5;
                    break;
                case 6:
                    ruletype.SelectedIndex = 4;
                    break;
                case 7:
                    ruletype.SelectedIndex = 0;
                    break;
                case 8:
                    ruletype.SelectedIndex = 0;
                    break;
                case 9:
                    ruletype.SelectedIndex = 3;
                    break;
                case 10:
                    ruletype.SelectedIndex = 2;
                    break;
                default:
                    ruletype.SelectedIndex = 0;
                    break;
            }


            prefinput.ItemsSource = Inputs;

            if (CurrentRule.prefinput == 0)
            {
                prefinput.SelectedIndex = 0;
            }
            else
            {
                int i = 1;

                for (i = 1; i < Inputs.Count; i++)
                {
                    if (Inputs[i].Content == CurrentRule.prefinput.ToString())
                        prefinput.SelectedIndex = i;
                }
            }

            //MessageBox.Show("Have inputs: " + Inputs.Count);

            recpriority.Text = CurrentRule.recpriority.ToString();
            inactive.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.inactive);
            autoexpire.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.autoexpire);
            maxnewest.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.maxnewest);
            maxepisodes.Text = CurrentRule.maxepisodes.ToString();
            startoffset.Text = CurrentRule.startoffset.ToString();
            endoffset.Text = CurrentRule.endoffset.ToString();

            autometalookup.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.autometalookup);
            autocommflag.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.autocommflag);
            autotranscode.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.autotranscode);
            autouserjob1.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.autouserjob1);
            autouserjob2.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.autouserjob2);
            autouserjob3.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.autouserjob3);
            autouserjob4.IsChecked = App.ViewModel.functions.IntToBool(CurrentRule.autouserjob4);


            performanceProgressBarCustomized.IsIndeterminate = false;

            HasLoaded = true;
        }


        private void CreateNewRule()
        {

            NewRule = new RuleViewModel();

            int value;

            if(!int.TryParse(endoffset.Text, out value))
            {
                MessageBox.Show("The end late value must be an integer");
                NewRule = null;
                return;
            }
            else if(!int.TryParse(maxepisodes.Text, out value))
            {
                MessageBox.Show("The maximum episodes value must be an integer");
                NewRule = null;
                return;
            }
            else if (!int.TryParse(recpriority.Text, out value))
            {
                MessageBox.Show("The recording priority value must be an integer");
                NewRule = null;
                return;
            }
            else if (!int.TryParse(startoffset.Text, out value))
            {
                MessageBox.Show("The start early episodes value must be an integer");
                NewRule = null;
                return;
            }



            NewRule.autometalookup = App.ViewModel.functions.BoolToInt((bool)autometalookup.IsChecked);
            NewRule.autocommflag = App.ViewModel.functions.BoolToInt((bool)autocommflag.IsChecked);
            NewRule.autoexpire = App.ViewModel.functions.BoolToInt((bool)autoexpire.IsChecked);
            NewRule.autotranscode = App.ViewModel.functions.BoolToInt((bool)autotranscode.IsChecked);
            NewRule.autouserjob1 = App.ViewModel.functions.BoolToInt((bool)autouserjob1.IsChecked);
            NewRule.autouserjob2 = App.ViewModel.functions.BoolToInt((bool)autouserjob2.IsChecked);
            NewRule.autouserjob3 = App.ViewModel.functions.BoolToInt((bool)autouserjob3.IsChecked);
            NewRule.autouserjob4 = App.ViewModel.functions.BoolToInt((bool)autouserjob4.IsChecked);

            NewRule.category = App.ViewModel.SelectedSetupProgram.category;
            NewRule.chanid = App.ViewModel.SelectedSetupProgram.chanid.ToString();
            NewRule.description = App.ViewModel.SelectedSetupProgram.description;
            NewRule.dupin = CurrentRule.dupin;
            NewRule.dupmethod = CurrentRule.dupmethod;
            NewRule.enddate = App.ViewModel.SelectedSetupProgram.endtime.Substring(0, 10);
            NewRule.endoffset = int.Parse(endoffset.Text);
            NewRule.endtime = App.ViewModel.SelectedSetupProgram.endtime.Substring(11, 8);

            NewRule.inactive = App.ViewModel.functions.BoolToInt((bool)inactive.IsChecked);
            NewRule.maxepisodes = int.Parse(maxepisodes.Text);
            NewRule.maxnewest = App.ViewModel.functions.BoolToInt((bool)maxnewest.IsChecked);
            NewRule.parentid = 0;
            NewRule.playgroup = CurrentRule.playgroup;
            NewRule.profile = CurrentRule.profile;
            NewRule.programid = App.ViewModel.SelectedSetupProgram.programid;

            NewRule.recgroup = CurrentRule.recgroup;
            NewRule.recpriority = int.Parse(recpriority.Text);
            NewRule.search = CurrentRule.search;
            NewRule.seriesid = App.ViewModel.SelectedSetupProgram.seriesid;
            NewRule.startdate = App.ViewModel.SelectedSetupProgram.starttime.Substring(0, 10);
            NewRule.startoffset = int.Parse(startoffset.Text);
            NewRule.starttime = App.ViewModel.SelectedSetupProgram.starttime.Substring(11, 8);

            NewRule.station = App.ViewModel.SelectedSetupProgram.callsign;
            NewRule.storagegroup = CurrentRule.storagegroup;
            NewRule.subtitle = App.ViewModel.SelectedSetupProgram.subtitle;
            NewRule.title = App.ViewModel.SelectedSetupProgram.title;
            NewRule.transcoder = CurrentRule.transcoder;

            NewRule.season = App.ViewModel.SelectedSetupProgram.season;
            NewRule.episode = App.ViewModel.SelectedSetupProgram.episode;
            NewRule.inetref = App.ViewModel.SelectedSetupProgram.inetref;
            NewRule.filter = CurrentRule.filter;

            /*
            NewRule.prefinput
            NewRule.recordid
            NewRule.type
             */

            var s = (NameContentViewModel)ruletype.SelectedItem;
            NewRule.type = int.Parse(s.Content);

            var t = (NameContentViewModel)prefinput.SelectedItem;
            NewRule.prefinput = int.Parse(t.Content);

            NewRule.recordid = CurrentRule.recordid;

            /*
             to cause debugger break 
             
            int x = 2;
            int y = 4;
            int z = 1 / (y - 2 * x);
             
             */

            return;

        }


        private void SaveRule()
        {
            //MessageBox.Show("SaveRule");

            if (NewRule == null)
                this.CreateNewRule();

            if (NewRule == null)
                return;

            if (NewRule.recordid == -1)
            {
                this.InsertRule();
            }
            else
            {
                savingPopup.IsOpen = true;
                performanceProgressBarCustomized.IsIndeterminate = true;

                this.SchedulerRule = NewRule.recordid;

                if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //MessageBox.Show("Not yet supprt in 0.25");

                    });

                    //With services API adding at same time seems to just update rule, so we can use the add API 

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(addRule25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting)));
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";

                    // Start the request
                    webRequest.BeginGetRequestStream(new AsyncCallback(AddRuleStreamCallback), webRequest);
                }
                else
                {

                    try
                    {

                        string query = "UPDATE `record` SET ";
                        query += "type = " + NewRule.type;
                        query += ", title = \"" + NewRule.title;
                        query += "\", subtitle = \"" + NewRule.subtitle;

                        query += "\", startdate = '" + NewRule.startdate;
                        query += "', starttime = '" + NewRule.starttime;
                        query += "', station = \"" + NewRule.station;

                        query += "\", description = \"" + NewRule.description;
                        query += "\", category = \"" + NewRule.category;
                        query += "\", seriesid = '" + NewRule.seriesid;

                        query += "', programid = '" + NewRule.programid;
                        query += "', chanid = '" + NewRule.chanid;
                        query += "', endtime = '" + NewRule.endtime;

                        query += "', enddate = '" + NewRule.enddate;
                        query += "', profile = '" + NewRule.profile;
                        query += "', transcoder = '" + NewRule.transcoder;

                        query += "', recgroup = '" + NewRule.recgroup;
                        query += "', storagegroup = '" + NewRule.storagegroup;
                        query += "', playgroup = '" + NewRule.playgroup;

                        query += "', recpriority = '" + NewRule.recpriority;
                        query += "', dupmethod = '" + NewRule.dupmethod;

                        query += "', dupin = '" + NewRule.dupin;
                        query += "', prefinput = '" + NewRule.prefinput;
                        query += "', inactive = '" + NewRule.inactive;

                        query += "', autoexpire = '" + NewRule.autoexpire;
                        query += "', maxnewest = '" + NewRule.maxnewest;
                        query += "', maxepisodes = '" + NewRule.maxepisodes;

                        query += "', startoffset = '" + NewRule.startoffset;
                        query += "', endoffset = '" + NewRule.endoffset;
                        query += "', autocommflag = '" + NewRule.autocommflag;

                        query += "', autotranscode = '" + NewRule.autotranscode;
                        query += "', autouserjob1 = '" + NewRule.autouserjob1;
                        query += "', autouserjob2 = '" + NewRule.autouserjob2;

                        query += "', autouserjob3 = '" + NewRule.autouserjob3;
                        query += "', autouserjob4 = '" + NewRule.autouserjob4;

                        query += "' WHERE recordid = " + NewRule.recordid;
                        query += " LIMIT 1; ";

                        //MessageBox.Show("Query: " + query);

                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                        webRequest.BeginGetResponse(new AsyncCallback(SaveCallback), webRequest);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving rule: " + ex.ToString());
                    }
                }
            }
        }
        private void SaveCallback(IAsyncResult asynchronousResult)
        {
            //response not useful, just reschedule

            this.Reschedule();
        }

        private void InsertRule()
        {
            //MessageBox.Show("InsertRule");

            if (NewRule == null)
                this.CreateNewRule();

            if (NewRule == null)
                return;

            if (NewRule.type == 0)
            {
                MessageBox.Show("You must select a recording rule type.");
            }
            else
            {
                savingPopup.IsOpen = true;
                performanceProgressBarCustomized.IsIndeterminate = true;

                this.SchedulerRule = -1;


                if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
                {

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(addRule25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting)));
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";

                    // Start the request
                    webRequest.BeginGetRequestStream(new AsyncCallback(AddRuleStreamCallback), webRequest);
                }
                else
                {

                    try
                    {

                        string query = "INSERT INTO `record` SET ";
                        query += "type = " + NewRule.type;
                        query += ", title = \"" + NewRule.title;
                        query += "\", subtitle = \"" + NewRule.subtitle;

                        query += "\", startdate = '" + NewRule.startdate;
                        query += "', starttime = '" + NewRule.starttime;
                        query += "', station = \"" + NewRule.station;

                        query += "\", description = \"" + NewRule.description;
                        query += "\", category = \"" + NewRule.category;
                        query += "\", seriesid = '" + NewRule.seriesid;

                        query += "', programid = '" + NewRule.programid;
                        query += "', chanid = '" + NewRule.chanid;
                        query += "', endtime = '" + NewRule.endtime;

                        query += "', enddate = '" + NewRule.enddate;
                        query += "', profile = '" + NewRule.profile;
                        query += "', transcoder = '" + NewRule.transcoder;

                        query += "', recgroup = '" + NewRule.recgroup;
                        query += "', storagegroup = '" + NewRule.storagegroup;
                        query += "', playgroup = '" + NewRule.playgroup;

                        query += "', recpriority = '" + NewRule.recpriority;
                        query += "', dupmethod = '" + NewRule.dupmethod;

                        query += "', dupin = '" + NewRule.dupin;
                        query += "', prefinput = '" + NewRule.prefinput;
                        query += "', inactive = '" + NewRule.inactive;

                        query += "', autoexpire = '" + NewRule.autoexpire;
                        query += "', maxnewest = '" + NewRule.maxnewest;
                        query += "', maxepisodes = '" + NewRule.maxepisodes;

                        query += "', startoffset = '" + NewRule.startoffset;
                        query += "', endoffset = '" + NewRule.endoffset;
                        query += "', autocommflag = '" + NewRule.autocommflag;

                        query += "', autotranscode = '" + NewRule.autotranscode;
                        query += "', autouserjob1 = '" + NewRule.autouserjob1;
                        query += "', autouserjob2 = '" + NewRule.autouserjob2;

                        query += "', autouserjob3 = '" + NewRule.autouserjob3;
                        query += "', autouserjob4 = '" + NewRule.autouserjob4;

                        //query += "' WHERE recordid = " + NewRule.recordid;
                        //query += " LIMIT 1; ";
                        query += "' ;";

                        //MessageBox.Show("Query: " + query);

                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                        webRequest.BeginGetResponse(new AsyncCallback(InsertCallback), webRequest);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving rule: " + ex.ToString());
                    }
                }
            }

        }
        private void AddRuleStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            // End the stream request operation
            System.IO.Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            string newStartTime = DateTime.Parse(App.ViewModel.SelectedSetupProgram.starttime).ToUniversalTime().ToString("s");

            // Create the post data
            string postData = "";
            postData += "ChanId=" + NewRule.chanid;
            postData += "&StartTime=" + newStartTime;
            postData += "&ParentId=" + NewRule.parentid;
            postData += "&Inactive=" + App.ViewModel.functions.IntToBoolText(NewRule.inactive);
            postData += "&Season=" + NewRule.season;
            postData += "&Episode=" + NewRule.episode;
            postData += "&Inetref=" + NewRule.inetref;
            
            //postData += "&FindId=" + NewRule.findid;
            postData += "&Type=" + App.ViewModel.functions.IntToApiRecType(NewRule.type);
            postData += "&SearchType=" + "None";
            postData += "&RecPriority=" + NewRule.recpriority;
            postData += "&PreferredInput=" + NewRule.prefinput;
            postData += "&StartOffset=" + NewRule.startoffset;
            postData += "&EndOffset=" + NewRule.endoffset;
            //dupmethod
            //dupi
            postData += "&Filter=" + NewRule.filter;

            postData += "&RecProfile=" + NewRule.profile;
            postData += "&RecGroup=" + NewRule.recgroup;
            postData += "&StorageGroup=" + NewRule.storagegroup;
            postData += "&PlayGroup=" + NewRule.playgroup;
            postData += "&AutoExpire=" + App.ViewModel.functions.IntToBoolText(NewRule.autoexpire);
            postData += "&MaxEpisodes=" + NewRule.maxepisodes;
            postData += "&MaxNewest=" + App.ViewModel.functions.IntToBoolText(NewRule.maxnewest);

            postData += "&AutoCommflag=" + App.ViewModel.functions.IntToBoolText(NewRule.autocommflag);
            postData += "&AutoTranscode=" + App.ViewModel.functions.IntToBoolText(NewRule.autotranscode);
            postData += "&AutoMetaLookup=" + App.ViewModel.functions.IntToBoolText(NewRule.autometalookup);
            postData += "&AutoUserJob1=" + App.ViewModel.functions.IntToBoolText(NewRule.autouserjob1);
            postData += "&AutoUserJob2=" + App.ViewModel.functions.IntToBoolText(NewRule.autouserjob2);
            postData += "&AutoUserJob3=" + App.ViewModel.functions.IntToBoolText(NewRule.autouserjob3);
            postData += "&AutoUserJob4=" + App.ViewModel.functions.IntToBoolText(NewRule.autouserjob4);

            postData += "&Transcoder=" + NewRule.transcoder;


            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Add the post data to the web request
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request
            webRequest.BeginGetResponse(new AsyncCallback(AddRuleCallback), webRequest);
        }
        private void AddRuleCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response;

                // End the get response operation
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                System.IO.Stream streamResponse = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamResponse);
                var Response = streamReader.ReadToEnd();
                streamResponse.Close();
                streamReader.Close();
                response.Close();


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Success: " + Response, "SUCCESS", MessageBoxButton.OK);

                    this.Perform(() => ClosePage(), 10000);
                });

            }
            catch (WebException e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error: " + e.ToString(), "ERROR", MessageBoxButton.OK);
                });
            }
        }
        private void InsertCallback(IAsyncResult asynchronousResult)
        {
            //response not useful, just reschedule

            this.Reschedule();
        }

        private void DeleteRule()
        {
            
            savingPopup.IsOpen = true;
            performanceProgressBarCustomized.IsIndeterminate = true;

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(removeRule25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting)));
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                // Start the request
                webRequest.BeginGetRequestStream(new AsyncCallback(RemoveRuleStreamCallback), webRequest);
            }
            else
            {

                try
                {

                    string query = "DELETE FROM record WHERE recordid=" + CurrentRule.recordid.ToString() + " LIMIT 1; ";

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                    webRequest.BeginGetResponse(new AsyncCallback(DeleteCallback), webRequest);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting rule: " + ex.ToString());
                }
            }
        }
        private void RemoveRuleStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            // End the stream request operation
            System.IO.Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            // Create the post data
            // Demo POST data 
            string postData = "RecordId="+CurrentRule.recordid.ToString();

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Add the post data to the web request
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request
            webRequest.BeginGetResponse(new AsyncCallback(RemoveRuleCallback), webRequest);
        }
        private void RemoveRuleCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response;

                // End the get response operation
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                System.IO.Stream streamResponse = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamResponse);
                var Response = streamReader.ReadToEnd();
                streamResponse.Close();
                streamReader.Close();
                response.Close();


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Success: " + Response, "SUCCESS", MessageBoxButton.OK);

                    this.Perform(() => ClosePage(), 10000);
                });

            }
            catch (WebException e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error: " + e.ToString(), "ERROR", MessageBoxButton.OK);
                });
            }
        }
        private void DeleteCallback(IAsyncResult asynchronousResult)
        {
            //response not useful, just reschedule

            this.Reschedule();
        }

        private void ToggleRule()
        {
            
            savingPopup.IsOpen = true;
            performanceProgressBarCustomized.IsIndeterminate = true;


            int newRuleType = 7;

            if (CurrentRule.type == 7)
                newRuleType = 8;

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Not yet supprt in 0.25");

                });
            }
            else
            {

                try
                {

                    string query = "UPDATE record SET type = " + newRuleType.ToString() + " WHERE recordid=" + CurrentRule.recordid.ToString() + " LIMIT 1; ";

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                    webRequest.BeginGetResponse(new AsyncCallback(ToggleCallback), webRequest);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating rule: " + ex.ToString());
                }
            }
        }
        private void ToggleCallback(IAsyncResult asynchronousResult)
        {
            //response not useful, just reschedule

            this.Reschedule();
        }

        private void ForgetOld()
        {
            
            savingPopup.IsOpen = true;
            performanceProgressBarCustomized.IsIndeterminate = true;

            this.SchedulerRule = CurrentRule.recordid;


            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Not yet supprt in 0.25");

                });
            }
            else
            {

                try
                {

                    string query = "DELETE FROM `oldrecorded` WHERE title = \"";
                    query += App.ViewModel.SelectedSetupProgram.title;
                    query += "\" AND subtitle = \"";
                    query += App.ViewModel.SelectedSetupProgram.subtitle;
                    query += "\" AND description = \"";
                    query += App.ViewModel.SelectedSetupProgram.description;
                    query += "\" LIMIT 1; ";

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                    webRequest.BeginGetResponse(new AsyncCallback(ForgetOldCallback), webRequest);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting to forget old: " + ex.ToString());
                }
            }
        }
        private void ForgetOldCallback(IAsyncResult asynchronousResult)
        {
            //response not useful, just reschedule

            this.Reschedule();
        }

        private void NeverRecord()
        {

            savingPopup.IsOpen = true;
            performanceProgressBarCustomized.IsIndeterminate = true;

            this.SchedulerRule = CurrentRule.recordid;

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Not yet supprt in 0.25");

                });
            }
            else
            {

                try
                {

                    string query = "REPLACE INTO oldrecorded (chanid,starttime,endtime,title,";
                    query += "subtitle,description,category,seriesid,programid,";
                    query += "recordid,station,rectype,recstatus,duplicate) VALUES (";

                    query += App.ViewModel.SelectedSetupProgram.chanid + ",'";
                    query += App.ViewModel.SelectedSetupProgram.starttime.Replace("T", " ") + "','";
                    query += App.ViewModel.SelectedSetupProgram.endtime.Replace("T", " ") + "',\"";
                    query += App.ViewModel.SelectedSetupProgram.title + "\",\"";

                    query += App.ViewModel.SelectedSetupProgram.subtitle + "\",\"";
                    query += App.ViewModel.SelectedSetupProgram.description + "\",\"";
                    query += App.ViewModel.SelectedSetupProgram.category + "\",'";
                    query += App.ViewModel.SelectedSetupProgram.seriesid + "','";
                    query += App.ViewModel.SelectedSetupProgram.programid + "',";

                    query += CurrentRule.recordid + ",\"";
                    query += App.ViewModel.SelectedSetupProgram.callsign + "\",";
                    query += CurrentRule.type + ",";
                    query += "11,";
                    query += "1) ;";

                    //MessageBox.Show("query: " + query);

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=executeSQL&query64=" + Convert.ToBase64String(App.ViewModel.encoder.GetBytes(query)) + "&rand=" + App.ViewModel.randText()));
                    webRequest.BeginGetResponse(new AsyncCallback(NeverRecordCallback), webRequest);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting to never record: " + ex.ToString());
                }
            }
        }
        private void NeverRecordCallback(IAsyncResult asynchronousResult)
        {
            //response not useful, just reschedule

            this.Reschedule();
        }


        private void EnableRule()
        {

            savingPopup.IsOpen = true;
            performanceProgressBarCustomized.IsIndeterminate = true;

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(enableRule25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting)));
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                // Start the request
                webRequest.BeginGetRequestStream(new AsyncCallback(EnableRuleStreamCallback), webRequest);
            }
            else
            {

                MessageBox.Show("This option is not avaible for pre-0.25 versions 0f MythTV");
                
            }
        }
        private void DisableRule()
        {

            savingPopup.IsOpen = true;
            performanceProgressBarCustomized.IsIndeterminate = true;

            if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(disableRule25String, App.ViewModel.appSettings.MasterBackendIpSetting, App.ViewModel.appSettings.MasterBackendXmlPortSetting)));
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                // Start the request
                webRequest.BeginGetRequestStream(new AsyncCallback(EnableRuleStreamCallback), webRequest);
            }
            else
            {

                MessageBox.Show("This option is not avaible for pre-0.25 versions 0f MythTV");

            }
        }
        private void EnableRuleStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            // End the stream request operation
            System.IO.Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            // Create the post data
            // Demo POST data 
            string postData = "RecordId=" + CurrentRule.recordid.ToString();

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Add the post data to the web request
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request
            webRequest.BeginGetResponse(new AsyncCallback(EnableRuleCallback), webRequest);
        }
        private void EnableRuleCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response;

                // End the get response operation
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                System.IO.Stream streamResponse = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamResponse);
                var Response = streamReader.ReadToEnd();
                streamResponse.Close();
                streamReader.Close();
                response.Close();


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("Success: " + Response, "SUCCESS", MessageBoxButton.OK);

                    this.Perform(() => ClosePage(), 10000);
                });

            }
            catch (WebException e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error: " + e.ToString(), "ERROR", MessageBoxButton.OK);
                });
            }
        }

        private void Reschedule()
        {
            try
            {

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=reschedule&recordId=" + this.SchedulerRule.ToString() + "&rand=" + App.ViewModel.randText()));
                webRequest.BeginGetResponse(new AsyncCallback(RescheduleCallback), webRequest);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error requesting reschedule: " + ex.ToString());
            }

        }
        private void RescheduleCallback(IAsyncResult asynchronousResult)
        {
            //response not useful, just set timer to close page

            if(this.SchedulerRule == -1)
                this.Perform(() => ClosePage(), 12000);
            else
                this.Perform(() => ClosePage(), 6000);
        
        }


        private void ClosePage()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                savingPopup.IsOpen = false;
                performanceProgressBarCustomized.IsIndeterminate = false;

                if (HasLoaded) NavigationService.GoBack();
            });
        }


        private void Perform(Action myMethod, int delayInMilliseconds)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (s, e) => Thread.Sleep(delayInMilliseconds);

            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();

            worker.RunWorkerAsync();
        }

        private void appbarSave_Click(object sender, EventArgs e)
        {
            //this.CreateNewRule();

            this.SaveRule();
        }

        private void appbarDelete_Click(object sender, EventArgs e)
        {
            if ((CurrentRule.type == 7) || (CurrentRule.type == 8))
            {
                if (MessageBox.Show("Are you sure you want to delete this override rule and record normally?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    SchedulerRule = CurrentRule.recordid;

                    this.DeleteRule();
                }
            }
            else if (CurrentRule.type > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete this rule?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    SchedulerRule = CurrentRule.recordid;

                    this.DeleteRule();
                }
            }
            else
            {
                MessageBox.Show("You cannot delete a recording rule that does not exist");
            }
        }

        private void appbarBack_Click(object sender, EventArgs e)
        {
            this.ClosePage();
        }

        private void forceRecord_Click(object sender, EventArgs e)
        {
            
            if(CurrentRule.type == 7)
            {
                MessageBox.Show("This is already a 'force record' rule.");
            }
            else if(CurrentRule.type == 8)
            {
                this.SchedulerRule = CurrentRule.recordid;

                this.ToggleRule();
            }
            else if(CurrentRule.type == 0)
            {
                MessageBox.Show("You can only do a 'force record' if another rule already exists for this program.");
            }
            else
            {
                this.CreateNewRule();

                NewRule.type = 7;
                NewRule.parentid = App.ViewModel.SelectedSetupProgram.recordid;

                this.InsertRule();
            }
        }

        private void forceDontRecord_Click(object sender, EventArgs e)
        {
            if (CurrentRule.type == 7)
            {
                this.SchedulerRule = CurrentRule.recordid;

                this.ToggleRule();
            }
            else if (CurrentRule.type == 8)
            {
                MessageBox.Show("This is already a 'force dont record' rule.");
            }
            else if (CurrentRule.type == 0)
            {
                MessageBox.Show("You can only do a 'force dont record' if another rule already exists for this program.");
            }
            else
            {
                this.CreateNewRule();

                NewRule.type = 8;
                NewRule.parentid = App.ViewModel.SelectedSetupProgram.recordid;

                this.InsertRule();
            }
        }

        private void forgetOld_Click(object sender, EventArgs e)
        {
            if ((App.ViewModel.SelectedSetupProgram.recstatus == 11) || (App.ViewModel.SelectedSetupProgram.recstatus == 2))
            {
                //never record, previously recorded

                this.ForgetOld();
            }
            else
            {
                MessageBox.Show("You can only forget old recordings that are marked 'previously recorded' or 'never record'.");
            }
        }

        private void neverRecord_Click(object sender, EventArgs e)
        {

            if ((App.ViewModel.SelectedSetupProgram.recstatus != 0))
            {
                //no rule

                this.NeverRecord();
            }
            else
            {
                MessageBox.Show("You can only set a program matching an existing recording rule to 'never record'.");
            }
        }

        private void enableRule_Click(object sender, EventArgs e)
        {
            this.EnableRule();
        }

        private void disableRule_Click(object sender, EventArgs e)
        {
            this.DisableRule();
        }

        private void mythweb_Click(object sender, EventArgs e)
        {
            this.OpenMythweb();
        }
        private void OpenMythweb()
        {
            try
            {
                DateTime dateResult;
                DateTime.TryParse(App.ViewModel.SelectedSetupProgram.starttime, out dateResult);

                if ((App.ViewModel.SelectedSetupProgram.recstatus <= -2)&&(App.ViewModel.SelectedSetupProgram.recstatus >= -10)) 
                {
                    DateTime.TryParse(App.ViewModel.SelectedSetupProgram.recstartts, out dateResult);
                }

                //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
                TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
                //TimeSpan u = (dateResult - DateTime.Now);
                Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
                //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

                WebBrowserTask webopen = new WebBrowserTask();

                webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.WebserverHostSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedSetupProgram.chanid + "/" + timestamp);
                webopen.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening browser.  Check your webserver address in the preferences.");
            }
        }


    }
}