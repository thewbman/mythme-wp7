﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\Videos.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F593F33CEF9FABCB01E1C2FCB051FF0E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.261
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
    
    
    public partial class Videos : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal Microsoft.Phone.Controls.Pivot videoPivot;
        
        internal Microsoft.Phone.Controls.PivotItem AllTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector AllVideosLL;
        
        internal Microsoft.Phone.Controls.PivotItem RegularTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector RegularVideosLL;
        
        internal Microsoft.Phone.Controls.PivotItem SpecialsTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector SpecialsVideosLL;
        
        internal Microsoft.Phone.Controls.PivotItem TvTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector TvVideosLL;
        
        internal Microsoft.Phone.Controls.PivotItem YearTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector YearVideosLL;
        
        internal Microsoft.Phone.Controls.PivotItem RecentTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector RecentVideosLL;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/Videos.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.videoPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("videoPivot")));
            this.AllTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("AllTitle")));
            this.AllVideosLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("AllVideosLL")));
            this.RegularTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("RegularTitle")));
            this.RegularVideosLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("RegularVideosLL")));
            this.SpecialsTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("SpecialsTitle")));
            this.SpecialsVideosLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("SpecialsVideosLL")));
            this.TvTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("TvTitle")));
            this.TvVideosLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("TvVideosLL")));
            this.YearTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("YearTitle")));
            this.YearVideosLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("YearVideosLL")));
            this.RecentTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("RecentTitle")));
            this.RecentVideosLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("RecentVideosLL")));
            this.appbarRefresh = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarRefresh")));
        }
    }
}
