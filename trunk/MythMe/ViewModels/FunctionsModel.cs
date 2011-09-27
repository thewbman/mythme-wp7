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
				    newStatusText = " "+"No matching recording rule";
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
    }
}
