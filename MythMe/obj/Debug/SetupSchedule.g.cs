﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\SetupSchedule.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "20785D07E56ECB541CC30120DAC7695D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace MythMe {
    
    
    public partial class SetupSchedule : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal System.Windows.Controls.Primitives.Popup savingPopup;
        
        internal Microsoft.Phone.Controls.ListPicker ruletype;
        
        internal System.Windows.Controls.TextBlock recstatustext;
        
        internal System.Windows.Controls.TextBlock title;
        
        internal System.Windows.Controls.TextBlock subtitle;
        
        internal System.Windows.Controls.TextBlock category;
        
        internal System.Windows.Controls.TextBlock description;
        
        internal System.Windows.Controls.TextBlock starttime;
        
        internal System.Windows.Controls.TextBlock endtime;
        
        internal System.Windows.Controls.TextBlock seriesid;
        
        internal System.Windows.Controls.TextBlock programid;
        
        internal System.Windows.Controls.TextBlock channum;
        
        internal System.Windows.Controls.TextBlock channame;
        
        internal System.Windows.Controls.TextBlock chanid;
        
        internal System.Windows.Controls.TextBox recpriority;
        
        internal Microsoft.Phone.Controls.ListPicker prefinput;
        
        internal Microsoft.Phone.Controls.ToggleSwitch inactive;
        
        internal Microsoft.Phone.Controls.ToggleSwitch autoexpire;
        
        internal Microsoft.Phone.Controls.ToggleSwitch maxnewest;
        
        internal System.Windows.Controls.TextBox maxepisodes;
        
        internal System.Windows.Controls.TextBox startoffset;
        
        internal System.Windows.Controls.TextBox endoffset;
        
        internal Microsoft.Phone.Controls.ToggleSwitch autocommflag;
        
        internal Microsoft.Phone.Controls.ToggleSwitch autotranscode;
        
        internal Microsoft.Phone.Controls.ToggleSwitch autouserjob1;
        
        internal Microsoft.Phone.Controls.ToggleSwitch autouserjob2;
        
        internal Microsoft.Phone.Controls.ToggleSwitch autouserjob3;
        
        internal Microsoft.Phone.Controls.ToggleSwitch autouserjob4;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbarBack;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbarSave;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbarDelete;
        
        internal Microsoft.Phone.Shell.ApplicationBarMenuItem forceRecord;
        
        internal Microsoft.Phone.Shell.ApplicationBarMenuItem forceDontRecord;
        
        internal Microsoft.Phone.Shell.ApplicationBarMenuItem forgetOld;
        
        internal Microsoft.Phone.Shell.ApplicationBarMenuItem neverRecord;
        
        internal Microsoft.Phone.Shell.ApplicationBarMenuItem mythweb;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/SetupSchedule.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.savingPopup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("savingPopup")));
            this.ruletype = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("ruletype")));
            this.recstatustext = ((System.Windows.Controls.TextBlock)(this.FindName("recstatustext")));
            this.title = ((System.Windows.Controls.TextBlock)(this.FindName("title")));
            this.subtitle = ((System.Windows.Controls.TextBlock)(this.FindName("subtitle")));
            this.category = ((System.Windows.Controls.TextBlock)(this.FindName("category")));
            this.description = ((System.Windows.Controls.TextBlock)(this.FindName("description")));
            this.starttime = ((System.Windows.Controls.TextBlock)(this.FindName("starttime")));
            this.endtime = ((System.Windows.Controls.TextBlock)(this.FindName("endtime")));
            this.seriesid = ((System.Windows.Controls.TextBlock)(this.FindName("seriesid")));
            this.programid = ((System.Windows.Controls.TextBlock)(this.FindName("programid")));
            this.channum = ((System.Windows.Controls.TextBlock)(this.FindName("channum")));
            this.channame = ((System.Windows.Controls.TextBlock)(this.FindName("channame")));
            this.chanid = ((System.Windows.Controls.TextBlock)(this.FindName("chanid")));
            this.recpriority = ((System.Windows.Controls.TextBox)(this.FindName("recpriority")));
            this.prefinput = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("prefinput")));
            this.inactive = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("inactive")));
            this.autoexpire = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("autoexpire")));
            this.maxnewest = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("maxnewest")));
            this.maxepisodes = ((System.Windows.Controls.TextBox)(this.FindName("maxepisodes")));
            this.startoffset = ((System.Windows.Controls.TextBox)(this.FindName("startoffset")));
            this.endoffset = ((System.Windows.Controls.TextBox)(this.FindName("endoffset")));
            this.autocommflag = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("autocommflag")));
            this.autotranscode = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("autotranscode")));
            this.autouserjob1 = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("autouserjob1")));
            this.autouserjob2 = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("autouserjob2")));
            this.autouserjob3 = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("autouserjob3")));
            this.autouserjob4 = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("autouserjob4")));
            this.appbarBack = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarBack")));
            this.appbarSave = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarSave")));
            this.appbarDelete = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarDelete")));
            this.forceRecord = ((Microsoft.Phone.Shell.ApplicationBarMenuItem)(this.FindName("forceRecord")));
            this.forceDontRecord = ((Microsoft.Phone.Shell.ApplicationBarMenuItem)(this.FindName("forceDontRecord")));
            this.forgetOld = ((Microsoft.Phone.Shell.ApplicationBarMenuItem)(this.FindName("forgetOld")));
            this.neverRecord = ((Microsoft.Phone.Shell.ApplicationBarMenuItem)(this.FindName("neverRecord")));
            this.mythweb = ((Microsoft.Phone.Shell.ApplicationBarMenuItem)(this.FindName("mythweb")));
        }
    }
}

