﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\Log.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D13A172FBA450D8A43825E33E5CDBDBE"
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
    
    
    public partial class Log : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal Microsoft.Phone.Controls.LongListSelector AllLL;
        
        internal Microsoft.Phone.Controls.LongListSelector AutoexpireLL;
        
        internal Microsoft.Phone.Controls.LongListSelector CommflagLL;
        
        internal Microsoft.Phone.Controls.LongListSelector JobqueueLL;
        
        internal Microsoft.Phone.Controls.LongListSelector MythbackendLL;
        
        internal Microsoft.Phone.Controls.LongListSelector MythfilldatabaseLL;
        
        internal Microsoft.Phone.Controls.LongListSelector SchedulerLL;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbarRefresh;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/Log.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.AllLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("AllLL")));
            this.AutoexpireLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("AutoexpireLL")));
            this.CommflagLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("CommflagLL")));
            this.JobqueueLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("JobqueueLL")));
            this.MythbackendLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("MythbackendLL")));
            this.MythfilldatabaseLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("MythfilldatabaseLL")));
            this.SchedulerLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("SchedulerLL")));
            this.appbarRefresh = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarRefresh")));
        }
    }
}

