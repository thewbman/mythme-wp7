﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\Upcoming.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "06B823A2270BEFC37B424788644F3C2E"
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
    
    
    public partial class Upcoming : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal Microsoft.Phone.Controls.Pivot upcomingHeader;
        
        internal Microsoft.Phone.Controls.PivotItem UpcomingTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector UpcomingUpcomingLL;
        
        internal Microsoft.Phone.Controls.PivotItem AllTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector AllUpcomingLL;
        
        internal Microsoft.Phone.Controls.PivotItem ConflictingTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector ConflictingUpcomingLL;
        
        internal Microsoft.Phone.Controls.PivotItem OverridesTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector OverridesUpcomingLL;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/Upcoming.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.upcomingHeader = ((Microsoft.Phone.Controls.Pivot)(this.FindName("upcomingHeader")));
            this.UpcomingTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("UpcomingTitle")));
            this.UpcomingUpcomingLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("UpcomingUpcomingLL")));
            this.AllTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("AllTitle")));
            this.AllUpcomingLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("AllUpcomingLL")));
            this.ConflictingTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("ConflictingTitle")));
            this.ConflictingUpcomingLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("ConflictingUpcomingLL")));
            this.OverridesTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("OverridesTitle")));
            this.OverridesUpcomingLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("OverridesUpcomingLL")));
            this.appbarRefresh = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarRefresh")));
        }
    }
}

