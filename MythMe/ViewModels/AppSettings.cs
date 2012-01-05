using System;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Collections.Generic;

namespace MythMe
{
    public class AppSettings
    {
        // Our isolated storage settings
        IsolatedStorageSettings settings;

        // The isolated storage key names of our settings
        const string FirstRunName = "FirstRun";
        const string AppStartsName = "AppStarts";
        const string ReviewedName = "Reviewed";
        const string PromptScriptName = "PromptScript";

        const string MasterBackendIpName = "MasterBackendIp";
        const string MasterBackendIpSettingName = "MasterBackendIpSetting";
        const string MasterBackendPortName = "MasterBackendPort";
        const string MasterBackendXmlPortName = "MasterBackendXmlPort"; 
        
        const string ManualDatabaseName = "ManualDatabase"; 
        const string DatabaseHostName = "DatabaseHost"; 
        const string DatabasePortName = "DatabasePort"; 
        const string DatabaseUsernameName = "DatabaseUsername"; 
        const string DatabasePasswordName = "DatabasePassword"; 
        const string DatabaseNameName = "DatabaseName"; 
        
        const string WebserverHostName = "WebserverHost"; 
        const string WebserverUsernameName = "WebserverUsername"; 
        const string WebserverPasswordName = "WebserverPassword"; 
        const string UseScriptName = "UseScript";
        const string MythwebXmlName = "MythwebXml";
        const string MythwebXmlKeyName = "MythwebXmlKey";
        const string PythonFileName = "PythonFile";
        const string AllowDownloadsName = "AllowDownloads";
		
        const string ChannelIconsName = "ChannelIcons";
        const string VideoListImagesName = "VideoListImages";
        const string VideoDetailsImageName = "VideoDetailsImage";
        const string UseScriptScreenshotsName = "UserScriptScreenshots";

        const string RemoteIndexName = "RemoteIndex";

		const string RemoteHeaderName = "RemoteHeader";
        const string RemoteVibrateName = "RemoteVibrate";
		const string RemoteFullscreenName = "RemoteFullscreen";
        const string PlayJumpRemoteName = "PlayJumpRemote";
        const string LivetvJumpRemoteName = "LivetvJumpRemote";

        //remotePane: "Navigation",
		//recGroup: "Default",
        const string RecSortName = "RecSort";
        const string RecSortAscName = "RecSortAsc";
		//upcomingGroup: "Upcoming",
        const string GuideSortName = "GuideSort";
        const string GuideSortAscName = "GuideSortAsc";
        const string SearchSortName = "SearchSort";
        const string SearchSortAscName = "SearchSortAsc";
        const string PeopleSortName = "PeopleSort";
        const string PeopleSortAscName = "PeopleSortAsc";
		//videosSort: "Title [Asc][]:[]Season [Asc]",
		//videosGroup: "Directory",
		//musicSort: "artist-asc",	//asdf
		
		const string ProtoVerName = "ProtoVer";
        //protoVerSubmitted: false,
        const string MythBinaryName = "MythBinary";
        //mythXmlVer: "0.0",
		//guideXmlVer: "0.0",
		//dvrXmlVer: "0.0",
		//contentXmlVer: "0.0",
				
		const string AutoCommercialFlagName = "AutoCommercialFlag";
		const string AutoTranscodeName = "AutoTranscode";
		const string AutoRunUserJob1Name = "AutoRunUserJob1";
		const string AutoRunUserJob2Name = "AutoRunUserJob2";
		const string AutoRunUserJob3Name = "AutoRunUserJob3";
		const string AutoRunUserJob4Name = "AutoRunUserJob4";
		const string DefaultStartOffsetName = "DefaultStartOffset";
		const string DefaultEndOffsetName = "DefaultEndOffset";
		const string UserJobDesc1Name = "UserJobDesc1";
		const string UserJobDesc2Name = "UserJobDesc2";
		const string UserJobDesc3Name = "UserJobDesc3";
		const string UserJobDesc4Name = "UserJobDesc4";
		
		const string MusicSongsName = "MusicSongs";
        const string DBSchemaVerName = "DBSchemaVer";

        const string ThemeKeyName = "Theme";



        // The default value of our settings
        const bool FirstRunDefault = true;
        const int AppStartsDefault = 0;
        const bool ReviewedDefault = false;
        const bool PromptScriptDefault = false;


        const string MasterBackendIpDefault = "";
        const string MasterBackendIpSettingDefault = "";
        const int MasterBackendPortDefault = 6543;
        const int MasterBackendXmlPortDefault = 6544;

        const bool ManualDatabaseDefault = false;
        const string DatabaseHostDefault = "-";
        const int DatabasePortDefault = 3306;
        const string DatabaseUsernameDefault = "mythtv";
        const string DatabasePasswordDefault = "mythtv";
        const string DatabaseNameDefault = "mythconverg";

        const string WebserverHostDefault = "";
        const string WebserverUsernameDefault = "";
        const string WebserverPasswordDefault = "";
        const bool UseScriptDefault = false;
        const bool MythwebXmlDefault = false;
        const string MythwebXmlKeyDefault = "DefaultKey";
        const string PythonFileDefault = "/cgi-bin/webmyth.py";
        const bool AllowDownloadsDefault = false;

        const bool ChannelIconsDefault = true;
        const bool VideoListImagesDefault = false;
        const bool VideoDetailsImageDefault = true;
        const bool UseScriptScreenshotsDefault = false;

        const int RemoteIndexDefault = 0;

        const string RemoteHeaderDefault = "None";
        const bool RemoteVibrateDefault = false;
        const bool RemoteFullscreenDefault = false;
        const bool PlayJumpRemoteDefault = true;
        const bool LivetvJumpRemoteDefault = false;

        //remotePane: "Navigation",
        //recGroup: "Default",
        const string RecSortDefault = "date";
        const bool RecSortAscDefault = false;
        //upcomingGroup: "Upcoming",
        const string GuideSortDefault = "recstatus";
        const bool GuideSortAscDefault = false;
        const string SearchSortDefault = "starttime";
        const bool SearchSortAscDefault = true;
        const string PeopleSortDefault = "starttime";
        const bool PeopleSortAscDefault = true;
        //videosSort: "Title [Asc][]:[]Season [Asc]",
        //videosGroup: "Directory",
        //musicSort: "artist-asc",	//asdf

        const int ProtoVerDefault = 0;
        //protoVerSubmitted: false,
        const string MythBinaryDefault = "-";
        //mythXmlVer: "0.0",
        //guideXmlVer: "0.0",
        //dvrXmlVer: "0.0",
        //contentXmlVer: "0.0",

        const bool AutoCommercialFlagDefault = true;
        const bool AutoTranscodeDefault = false;
        const bool AutoRunUserJob1Default = false;
        const bool AutoRunUserJob2Default = false;
        const bool AutoRunUserJob3Default = false;
        const bool AutoRunUserJob4Default = false;
        const int DefaultStartOffsetDefault = 0;
        const int DefaultEndOffsetDefault = 0;
        const string UserJobDesc1Default = "UserJob1";
        const string UserJobDesc2Default = "UserJob2";
        const string UserJobDesc3Default = "UserJob3";
        const string UserJobDesc4Default = "UserJob4";

        const int MusicSongsDefault = 0;
        const int DBSchemaVerDefault = 0;

        const string ThemeDefault = "black";





        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public AppSettings()
        {
            // Get the settings for this application.
            settings = IsolatedStorageSettings.ApplicationSettings;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            settings.Save();
        }



        public bool FirstRunSetting
        {
            get { return GetValueOrDefault<bool>(FirstRunName, FirstRunDefault); }
            set { if (AddOrUpdateValue(FirstRunName, value)) { Save(); } }
        }
        public int AppStartsSetting
        {
            get { return GetValueOrDefault<int>(AppStartsName, AppStartsDefault); }
            set { if (AddOrUpdateValue(AppStartsName, value)) { Save(); } }
        }
        public bool ReviewedSetting
        {
            get { return GetValueOrDefault<bool>(ReviewedName, ReviewedDefault); }
            set { if (AddOrUpdateValue(ReviewedName, value)) { Save(); } }
        }
        public bool PromptScriptSetting
        {
            get { return GetValueOrDefault<bool>(PromptScriptName, PromptScriptDefault); }
            set { if (AddOrUpdateValue(PromptScriptName, value)) { Save(); } }
        }

        public string MasterBackendIpSetting
        {
            get { return GetValueOrDefault<string>(MasterBackendIpName, MasterBackendIpDefault); }
            set { if (AddOrUpdateValue(MasterBackendIpName, value)) { Save(); } }
        }
        public string MasterBackendIpSettingSetting
        {
            get { return GetValueOrDefault<string>(MasterBackendIpSettingName, MasterBackendIpSettingDefault); }
            set { if (AddOrUpdateValue(MasterBackendIpSettingName, value)) { Save(); } }
        }
        public int MasterBackendPortSetting
        {
            get { return GetValueOrDefault<int>(MasterBackendPortName, MasterBackendPortDefault); }
            set { if (AddOrUpdateValue(MasterBackendPortName, value)) { Save(); } }
        }
        public int MasterBackendXmlPortSetting
        {
            get { return GetValueOrDefault<int>(MasterBackendXmlPortName, MasterBackendXmlPortDefault); }
            set { if (AddOrUpdateValue(MasterBackendXmlPortName, value)) { Save(); } }
        }

        public bool ManualDatabaseSetting
        {
            get { return GetValueOrDefault<bool>(ManualDatabaseName, ManualDatabaseDefault); }
            set { if (AddOrUpdateValue(ManualDatabaseName, value)) { Save(); } }
        }
        public string DatabaseHostSetting
        {
            get { return GetValueOrDefault<string>(DatabaseHostName, DatabaseHostDefault); }
            set { if (AddOrUpdateValue(DatabaseHostName, value)) { Save(); } }
        }
        public int DatabasePortSetting
        {
            get { return GetValueOrDefault<int>(DatabasePortName, DatabasePortDefault); }
            set { if (AddOrUpdateValue(DatabasePortName, value)) { Save(); } }
        }
        public string DatabaseUsernameSetting
        {
            get { return GetValueOrDefault<string>(DatabaseUsernameName, DatabaseUsernameDefault); }
            set { if (AddOrUpdateValue(DatabaseUsernameName, value)) { Save(); } }
        }
        public string DatabasePasswordSetting
        {
            get { return GetValueOrDefault<string>(DatabasePasswordName, DatabasePasswordDefault); }
            set { if (AddOrUpdateValue(DatabasePasswordName, value)) { Save(); } }
        }
        public string DatabaseNameSetting
        {
            get { return GetValueOrDefault<string>(DatabaseNameName, DatabaseNameDefault); }
            set { if (AddOrUpdateValue(DatabaseNameName, value)) { Save(); } }
        }

        public string WebserverHostSetting
        {
            get { return GetValueOrDefault<string>(WebserverHostName, WebserverHostDefault); }
            set { if (AddOrUpdateValue(WebserverHostName, value)) { Save(); } }
        }
        public string WebserverUsernameSetting
        {
            get { return GetValueOrDefault<string>(WebserverUsernameName, WebserverUsernameDefault); }
            set { if (AddOrUpdateValue(WebserverUsernameName, value)) { Save(); } }
        }
        public string WebserverPasswordSetting
        {
            get { return GetValueOrDefault<string>(WebserverPasswordName, WebserverPasswordDefault); }
            set { if (AddOrUpdateValue(WebserverPasswordName, value)) { Save(); } }
        }
        public bool UseScriptSetting
        {
            get { return GetValueOrDefault<bool>(UseScriptName, UseScriptDefault); }
            set { if (AddOrUpdateValue(UseScriptName, value)) { Save(); } }
        }
        public bool MythwebXmlSetting
        {
            get { return GetValueOrDefault<bool>(MythwebXmlName, MythwebXmlDefault); }
            set { if (AddOrUpdateValue(MythwebXmlName, value)) { Save(); } }
        }
        public string MythwebXmlKeySetting
        {
            get { return GetValueOrDefault<string>(MythwebXmlKeyName, MythwebXmlKeyDefault); }
            set { if (AddOrUpdateValue(MythwebXmlKeyName, value)) { Save(); } }
        }
        public string PythonFileSetting
        {
            get { return GetValueOrDefault<string>(PythonFileName, PythonFileDefault); }
            set { if (AddOrUpdateValue(PythonFileName, value)) { Save(); } }
        }
        public bool AllowDownloadsSetting
        {
            get { return GetValueOrDefault<bool>(AllowDownloadsName, AllowDownloadsDefault); }
            set { if (AddOrUpdateValue(AllowDownloadsName, value)) { Save(); } }
        }

        public bool ChannelIconsSetting
        {
            get { return GetValueOrDefault<bool>(ChannelIconsName, ChannelIconsDefault); }
            set { if (AddOrUpdateValue(ChannelIconsName, value)) { Save(); } }
        }
        public bool VideoListImagesSetting
        {
            get { return GetValueOrDefault<bool>(VideoListImagesName, VideoListImagesDefault); }
            set { if (AddOrUpdateValue(VideoListImagesName, value)) { Save(); } }
        }
        public bool VideoDetailsImageSetting
        {
            get { return GetValueOrDefault<bool>(VideoDetailsImageName, VideoDetailsImageDefault); }
            set { if (AddOrUpdateValue(VideoDetailsImageName, value)) { Save(); } }
        }
        public bool UseScriptScreenshotsSetting
        {
            get { return GetValueOrDefault<bool>(UseScriptScreenshotsName, UseScriptScreenshotsDefault); }
            set { if (AddOrUpdateValue(UseScriptScreenshotsName, value)) { Save(); } }
        }

        public int RemoteIndexSetting
        {
            get { return GetValueOrDefault<int>(RemoteIndexName, RemoteIndexDefault); }
            set { if (AddOrUpdateValue(RemoteIndexName, value)) { Save(); } }
        }

        public string RemoteHeaderSetting
        {
            get { return GetValueOrDefault<string>(RemoteHeaderName, RemoteHeaderDefault); }
            set { if (AddOrUpdateValue(RemoteHeaderName, value)) { Save(); } }
        }
        public bool RemoteVibrateSetting
        {
            get { return GetValueOrDefault<bool>( RemoteVibrateName,  RemoteVibrateDefault); }
            set { if (AddOrUpdateValue( RemoteVibrateName, value)) { Save(); } }
        }
        public bool RemoteFullscreenSetting
        {
            get { return GetValueOrDefault<bool>(RemoteFullscreenName, RemoteFullscreenDefault); }
            set { if (AddOrUpdateValue(RemoteFullscreenName, value)) { Save(); } }
        }
        public bool PlayJumpRemoteSetting
        {
            get { return GetValueOrDefault<bool>(PlayJumpRemoteName, PlayJumpRemoteDefault); }
            set { if (AddOrUpdateValue(PlayJumpRemoteName, value)) { Save(); } }
        }
        public bool LivetvJumpRemoteSetting
        {
            get { return GetValueOrDefault<bool>(LivetvJumpRemoteName, LivetvJumpRemoteDefault); }
            set { if (AddOrUpdateValue(LivetvJumpRemoteName, value)) { Save(); } }
        }

        //remotePane: "Navigation",
        //recGroup: "Setting",
        public string RecSortSetting
        {
            get { return GetValueOrDefault<string>(RecSortName, RecSortDefault); }
            set { if (AddOrUpdateValue(RecSortName, value)) { Save(); } }
        }
        public bool RecSortAscSetting
        {
            get { return GetValueOrDefault<bool>(RecSortAscName, RecSortAscDefault); }
            set { if (AddOrUpdateValue(RecSortAscName, value)) { Save(); } }
        }
        //upcomingGroup: "Upcoming",
        public string GuideSortSetting
        {
            get { return GetValueOrDefault<string>(GuideSortName, GuideSortDefault); }
            set { if (AddOrUpdateValue(GuideSortName, value)) { Save(); } }
        }
        public bool GuideSortAscSetting
        {
            get { return GetValueOrDefault<bool>(GuideSortAscName, GuideSortAscDefault); }
            set { if (AddOrUpdateValue(GuideSortAscName, value)) { Save(); } }
        }
        public string SearchSortSetting
        {
            get { return GetValueOrDefault<string>(SearchSortName, SearchSortDefault); }
            set { if (AddOrUpdateValue(SearchSortName, value)) { Save(); } }
        }
        public bool SearchSortAscSetting
        {
            get { return GetValueOrDefault<bool>(SearchSortAscName, SearchSortAscDefault); }
            set { if (AddOrUpdateValue(SearchSortAscName, value)) { Save(); } }
        }
        public string PeopleSortSetting
        {
            get { return GetValueOrDefault<string>(PeopleSortName, PeopleSortDefault); }
            set { if (AddOrUpdateValue(PeopleSortName, value)) { Save(); } }
        }
        public bool PeopleSortAscSetting
        {
            get { return GetValueOrDefault<bool>(PeopleSortAscName, PeopleSortAscDefault); }
            set { if (AddOrUpdateValue(PeopleSortAscName, value)) { Save(); } }
        }
        //videosSort: "Title [Asc][]:[]Season [Asc]",
        //videosGroup: "Directory",
        //musicSort: "artist-asc",	//asdf

        public int ProtoVerSetting
        {
            get { return GetValueOrDefault<int>(ProtoVerName, ProtoVerDefault); }
            set { if (AddOrUpdateValue(ProtoVerName, value)) { Save(); } }
        }
        //protoVerSubmitted: false,
        public string MythBinarySetting
        {
            get { return GetValueOrDefault<string>(MythBinaryName, MythBinaryDefault); }
            set { if (AddOrUpdateValue(MythBinaryName, value)) { Save(); } }
        }
        //mythXmlVer: "0.0",
        //guideXmlVer: "0.0",
        //dvrXmlVer: "0.0",
        //contentXmlVer: "0.0",

        public bool AutoCommercialFlagSetting
        {
            get { return GetValueOrDefault<bool>(AutoCommercialFlagName, AutoCommercialFlagDefault); }
            set { if (AddOrUpdateValue(AutoCommercialFlagName, value)) { Save(); } }
        }
        public bool AutoTranscodeSetting
        {
            get { return GetValueOrDefault<bool>(AutoTranscodeName, AutoTranscodeDefault); }
            set { if (AddOrUpdateValue(AutoTranscodeName, value)) { Save(); } }
        }
        public bool AutoRunUserJob1Setting
        {
            get { return GetValueOrDefault<bool>(AutoRunUserJob1Name, AutoRunUserJob1Default); }
            set { if (AddOrUpdateValue(AutoRunUserJob1Name, value)) { Save(); } }
        }
        public bool AutoRunUserJob2Setting
        {
            get { return GetValueOrDefault<bool>(AutoRunUserJob2Name, AutoRunUserJob2Default); }
            set { if (AddOrUpdateValue(AutoRunUserJob2Name, value)) { Save(); } }
        }
        public bool AutoRunUserJob3Setting
        {
            get { return GetValueOrDefault<bool>(AutoRunUserJob3Name, AutoRunUserJob3Default); }
            set { if (AddOrUpdateValue(AutoRunUserJob3Name, value)) { Save(); } }
        }
        public bool AutoRunUserJob4Setting
        {
            get { return GetValueOrDefault<bool>(AutoRunUserJob4Name, AutoRunUserJob4Default); }
            set { if (AddOrUpdateValue(AutoRunUserJob4Name, value)) { Save(); } }
        }
        public int DefaultStartOffsetSetting
        {
            get { return GetValueOrDefault<int>(DefaultStartOffsetName, DefaultStartOffsetDefault); }
            set { if (AddOrUpdateValue(DefaultStartOffsetName, value)) { Save(); } }
        }
        public int DefaultEndOffsetSetting
        {
            get { return GetValueOrDefault<int>(DefaultEndOffsetName, DefaultEndOffsetDefault); }
            set { if (AddOrUpdateValue(DefaultEndOffsetName, value)) { Save(); } }
        }
        public string UserJobDesc1Setting
        {
            get { return GetValueOrDefault<string>( UserJobDesc1Name,  UserJobDesc1Default); }
            set { if (AddOrUpdateValue( UserJobDesc1Name, value)) { Save(); } }
        }
        public string UserJobDesc2Setting
        {
            get { return GetValueOrDefault<string>(UserJobDesc2Name, UserJobDesc2Default); }
            set { if (AddOrUpdateValue(UserJobDesc2Name, value)) { Save(); } }
        }
        public string UserJobDesc3Setting
        {
            get { return GetValueOrDefault<string>(UserJobDesc3Name, UserJobDesc3Default); }
            set { if (AddOrUpdateValue(UserJobDesc3Name, value)) { Save(); } }
        }
        public string UserJobDesc4Setting
        {
            get { return GetValueOrDefault<string>(UserJobDesc4Name, UserJobDesc4Default); }
            set { if (AddOrUpdateValue(UserJobDesc4Name, value)) { Save(); } }
        }

        public int MusicSongsSetting
        {
            get { return GetValueOrDefault<int>(MusicSongsName, MusicSongsDefault); }
            set { if (AddOrUpdateValue(MusicSongsName, value)) { Save(); } }
        }
        public int DBSchemaVerSetting
        {
            get { return GetValueOrDefault<int>(DBSchemaVerName, DBSchemaVerDefault); }
            set { if (AddOrUpdateValue(DBSchemaVerName, value)) { Save(); } }
        }

        public string ThemeSetting
        {
            get { return GetValueOrDefault<string>(ThemeKeyName, ThemeDefault); }
            set { if (AddOrUpdateValue(ThemeKeyName, value)) { Save(); } }
        }

        
    }
}