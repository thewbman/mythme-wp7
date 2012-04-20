using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace MythMe
{
    public class FunctionsModel
    {

        public string RecStatusDecode(int recstatus)
        { 
            string newStatusText;
            
            switch(recstatus)
            {
                case -10:		
				    newStatusText = "Tuning";
			    break;
			    case -9:		
				    newStatusText = "Failed";
			    break;
			    case -8:		
				    newStatusText = "Tuner Busy";
			    break;
			    case -7:		
				    newStatusText = "Low Disk Space";
			    break;
			    case -6:		
				    newStatusText = "Cancelled";
			    break;
			    case -5:		
				    newStatusText = "Deleted";
			    break;
			    case -4:		
				    newStatusText = "Aborted";
			    break;
			    case -3:		
				    newStatusText = "Recorded";
			    break;
			    case -2:		
				    newStatusText = "Currently Recording";
			    break;
			    case -1:		
				    newStatusText = "Will Record";
			    break;
			    case 0:		
				    newStatusText = " No matching recording rule";
			    break;
			    case 1:		
				    newStatusText = "Force Don't Record";
			    break;
			    case 2:		
				    newStatusText = "Previously Recorded";
			    break;
			    case 3:		
				    newStatusText = "Current Recording";
			    break;
			    case 4:		
				    newStatusText = "Don't Record";
			    break;
			    case 5:		
				    newStatusText = "Earlier Showing";
			    break;
			    case 6:		
				    newStatusText = "Not Listed";
			    break;
			    case 7:		
				    newStatusText = "Conflict";
			    break;
			    case 8:		
				    newStatusText = "Later Showing";
			    break;
			    case 9:		
				    newStatusText = "Repeat";
			    break;
			    case 10:		
				    newStatusText = "Inactive";
			    break;
			    case 11:		
				    newStatusText = "Never Record";
			    break;
			
			    case 100:		
				    newStatusText = "";
				    //Blank for when using defaultProgram
			    break;
			
			    default:
				    newStatusText = " No matching recording rule";
			    break;
            }

            return newStatusText;
        }

        public string PadProtocol(string inCommand)
        {
            int l = inCommand.Length;
            string response = "";

            if(l < 10) {
                response = l + "       ";
            } else if (l < 100) {
                response = l + "      ";
            } else if (l < 1000) {
                response = l + "     ";
            } else if (l < 10000) {
                response = l + "    ";
            } else if (l < 100000) {
                response = l + "   ";
            } else if (l < 1000000) {
                response = l + "  ";
            }

            response += inCommand;

            return response;

        }

        public string ProtoVersion()
        {
            string response;

            int inProto = App.ViewModel.appSettings.ProtoVerSetting;

            switch (inProto)
            {
                case 72:
                    response = "MYTH_PROTO_VERSION 72 D78EFD6F";
                    break;
                case 71:
                    response = "MYTH_PROTO_VERSION 71 05e82186";
                    break;
                case 70:
                    response = "MYTH_PROTO_VERSION 70 53153836";
                    break;
                case 69:
                    response = "MYTH_PROTO_VERSION 69 63835135";
                    break;
                case 68:
                    response = "MYTH_PROTO_VERSION 68 90094EAD";
                    break;
                case 67:
                    response = "MYTH_PROTO_VERSION 67 0G0G0G0";
                    break;
                case 66:
                    response = "MYTH_PROTO_VERSION 66 0C0FFEE0";
                    break;
                case 65:
                    response = "MYTH_PROTO_VERSION 65 D2BB94C2";
                    break;
                case 64:
                    response = "MYTH_PROTO_VERSION 64 8675309J";
                    break;
                case 63:
                    response = "MYTH_PROTO_VERSION 63 3875641D";
                    break;
                case 62:
                    response = "MYTH_PROTO_VERSION 62 78B5631E";
                    break;
                //
                default:
                    response = "MYTH_PROTO_VERSION " + inProto;
                    break;
            }

            return response;
        }

        public string EncoderStateDecode(string inState)
        {
            string response;

            switch (int.Parse(inState))
            {
		        case -1:
			        response = "Disconnected";
			        break;
		        case 0:
			        response = "Idle";
			        break;
		        case 1:
			        response = "Watching Live TV";
			        break;
		        case 2:
			        response = "Watching Pre-Recorded";
			        break;
		        case 3:
			        response = "Watching Video";
			        break;
		        case 4:
			        response = "Watching DVD";
			        break;
		        case 5:
			        response = "Watching BD";
			        break;
		        case 6:
			        response = "Recording";
			        break;
		        case 7:
			        response = "Recording";
			        break;
		        case 8:
			        response = "Unknown Status 8";
			        break;
		        case 9:
			        response = "Unknown Status 9";
			        break;
                //
		        default:
			        response = "Unknown";
			        break;
            }

            return response;
        }

        public List<NameContentViewModel> JobsToModel(List<JobqueueModel> inJobs)
        {
            List<NameContentViewModel> l = new List<NameContentViewModel>();
            NameContentViewModel n = new NameContentViewModel();

            foreach (JobqueueModel j in inJobs)
            {
                n = new NameContentViewModel();

                n.Name = j.type;
                n.Content = JobqueueTypeDecode(j.type);
                n.First = JobqueueStatusDecode(j.status);
                n.Second = j.hostname;
                n.Third = j.comment;

                l.Add(n);
            }

            return l;
        }

        public string JobqueueTypeDecode(string inType)
        {
	        string jobType = "Unknown";

	        switch(int.Parse(inType)) {
		        case 0:
			        jobType = "System Job";
			        break;
		        case 1:
			        jobType = "Transcode";
			        break;
		        case 2:
			        jobType = "Commercial Flagging";
			        break;
		        case 256:
			        jobType = App.ViewModel.appSettings.UserJobDesc1Setting;
			        break;
                case 512:
                    jobType = App.ViewModel.appSettings.UserJobDesc2Setting;
			        break;
                case 1024:
                    jobType = App.ViewModel.appSettings.UserJobDesc3Setting;
			        break;
                case 2048:
                    jobType = App.ViewModel.appSettings.UserJobDesc4Setting;
			        break;
		        default:
			        jobType = "Unknown Job Type";
			        break;
	        };
	
	        return jobType;
        }

        public string JobqueueStatusDecode(string inStatus)
        {
            string statusText = "Unknown";

	        switch(int.Parse(inStatus)) {
		        case -100:
			        statusText = "";
			        break;
		        case 0:
			        statusText = "Unknown";
			        break;
		        case 1:
			        statusText = "Queued";
			        break;
		        case 2:
			        statusText = "Pending";
			        break;
		        case 3:
			        statusText = "Starting";
			        break;
		        case 4:
			        statusText = "Running";
			        break;
		        case 5:
			        statusText = "Stopped";
			        break;
		        case 6:
			        statusText = "Paused";
			        break;
		        case 7:
			        statusText = "Retry";
			        break;
		        case 8:
			        statusText = "Erroring";
			        break;
		        case 9:
			        statusText = "Aborting";
			        break;
		        case 256:
			        statusText = "Done";
			        break;
		        case 272:
			        statusText = "Finished";
			        break;
		        case 288:
			        statusText = "Aborted";
			        break;
		        case 304:
			        statusText = "Errored";
			        break;
		        case 320:
			        statusText = "Cancelled";
			        break;
		        default:
			        statusText = "Unknown";
			        break;
	        };
	
	        return statusText;
        }

        public bool IntToBool(string inValue)
        {
            if (inValue == "0")
                return false;
            else
                return true;
        }
        public bool IntToBool(int inValue)
        {
            if (inValue == 0)
                return false;
            else
                return true;
        }

        public string IntToBoolText(string inValue)
        {
            if (inValue == "0")
                return "false";
            else
                return "true";
        }
        public string IntToBoolText(int inValue)
        {
            if (inValue == 0)
                return "false";
            else
                return "true";
        }

        public int BoolToInt(bool inValue)
        {
            if (inValue == false)
                return 0;
            else
                return 1;
        }

        public int BoolTextToInt(string inValue)
        {
            if (inValue.ToLower() == "false")
                return 0;
            else
                return 1;
        }

        public int ApiRecTypeToInt(string inValue, int inRecstatus)
        {
            int type = -1;

            /*
                RuleTypes.Add(new NameContentViewModel(){Name = "Anytime on any channel", Content = "4"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Anytime on this channel", Content = "3"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one each week", Content = "10"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one each day", Content = "9"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Force dont record", Content = "8"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Force record", Content = "7"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one showing", Content = "6"});
                RuleTypes.Add(new NameContentViewModel(){Name = "This timeslot each week", Content = "5"});
                RuleTypes.Add(new NameContentViewModel(){Name = "This timeslot each day", Content = "2"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Only this showing", Content = "1"});
                RuleTypes.Add(new NameContentViewModel(){Name = "No recording rule", Content = "0"});
             */


            switch (inValue.ToLower())
            {
                case "find weekly":
                    type = 10;
                    break;
                case "find daily":
                    type = 9;
                    break;
                case "override recording":
                    if (inRecstatus == 1)
                    {
                        //force record
                        type = 7;
                    }
                    else
                    {
                        type = 8;
                    }
                    break;
                case "find one":
                    type = 6;
                    break;
                case "record weekly":
                    type = 5;
                    break;
                case "record all":
                    type = 4;
                    break;
                case "channel record":
                    type = 3;
                    break;
                case "record daily":
                    type = 2;
                    break;
                case "single record":
                    type = 1;
                    break;
                case "not recording":
                    type = 0;
                    break;
            }


            return type;
        }

        public string IntToApiRecType(int inValue)
        {
            string type = "not recording";

            /*
                RuleTypes.Add(new NameContentViewModel(){Name = "Anytime on any channel", Content = "4"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Anytime on this channel", Content = "3"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one each week", Content = "10"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one each day", Content = "9"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Force dont record", Content = "8"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Force record", Content = "7"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Find one showing", Content = "6"});
                RuleTypes.Add(new NameContentViewModel(){Name = "This timeslot each week", Content = "5"});
                RuleTypes.Add(new NameContentViewModel(){Name = "This timeslot each day", Content = "2"});
                RuleTypes.Add(new NameContentViewModel(){Name = "Only this showing", Content = "1"});
                RuleTypes.Add(new NameContentViewModel(){Name = "No recording rule", Content = "0"});
             */


            switch (inValue)
            {
                case 10:
                    type = "Find Weekly";
                    break;
                case 9:
                    type = "Find Daily";
                    break;
                case 8:
                    type = "Dont Record";       //not actual text, but is the final else statement  http://code.mythtv.org/doxygen/recordingtypes_8cpp_source.html
                    break;
                case 7:
                    type = "Override Recording";
                    break;
                case 6:
                    type = "Find One";
                    break;
                case 5:
                    type = "Record Weekly";
                    break;
                case 4:
                    type = "Record All";
                    break;
                case 3:
                    type = "Channel Record";
                    break;
                case 2:
                    type = "Record Daily";
                    break;
                case 1:
                    type = "Single Record";
                    break;
                case 0:
                    type = "Not Recording";
                    break;
            }


            return type;
        }

        public void FrontendsFromBackends()
        {

            App.ViewModel.Frontends.Clear();

            foreach (BackendViewModel backend in App.ViewModel.Backends)
            {
                if (backend.NetworkControlEnabled)
                {
                    FrontendViewModel newFrontend = new FrontendViewModel();

                    newFrontend.Name = backend.Name;
                    newFrontend.Port = backend.NetworkControlPort;
                    newFrontend.Address = backend.Address;

                    if (newFrontend.Address == null) newFrontend.Address = App.ViewModel.appSettings.MasterBackendIpSetting;

                    App.ViewModel.Frontends.Add(newFrontend);
                }
            }

            App.ViewModel.saveFrontends();

            App.ViewModel.appSettings.RemoteIndexSetting = 0;
        }

        public string CreateScreenshotUrl(ProgramViewModel inProgram)
        {
            string screenshot;

            if (App.ViewModel.appSettings.UseScriptScreenshotsSetting)
            {
                screenshot = "http://" + App.ViewModel.appSettings.WebserverHostSetting + "/cgi-bin/webmyth.py?op=getPremadeImage&chanid=";
                screenshot += inProgram.chanid + "&starttime=" + inProgram.recstartts.Replace("T", " ");
            }
            else
            {
                string hostaddress = App.ViewModel.appSettings.MasterBackendIpSetting;
                int hostport = App.ViewModel.appSettings.MasterBackendXmlPortSetting;

                foreach (BackendViewModel backend in App.ViewModel.Backends)
                {
                    if (backend.Name == inProgram.hostname) 
                    {
                        hostaddress = backend.Address;
                        hostport = backend.XmlPort;
                    }
                }

                if (App.ViewModel.appSettings.DBSchemaVerSetting > 1269)
                {
                    string newStartTime = DateTime.Parse(inProgram.recstartts).ToUniversalTime().ToString("s");

                    screenshot = "http://" + hostaddress + ":" + hostport + "/Content/GetPreviewImage?ChanId=";
                    screenshot += inProgram.chanid + "&StartTime=" + newStartTime.Replace("T", " ");
                }
                else
                {
                    screenshot = "http://" + hostaddress + ":" + hostport + "/Myth/GetPreviewImage?ChanId=";
                    screenshot += inProgram.chanid + "&StartTime=" + inProgram.recstartts.Replace("T", " ");
                }
            }

            return screenshot;

        }

        public string FirstChar(string inString)
        {
            string a = inString.ToUpper().Substring(0, 1);

            if (Regex.IsMatch(a, "[0-9]"))
            {
                a = "#";
            }
            else if (Regex.IsMatch(a, "[A-Z]"))
            {
                //
            }
            else
            {
                a = "~";
            }

            return a;
        }
        


    }
}
