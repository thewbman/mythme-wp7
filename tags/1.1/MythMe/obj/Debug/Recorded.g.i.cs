﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\Recorded.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "93D9CB359C283843A389BC5391C9CC7C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.237
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
    
    
    public partial class Recorded : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Primitives.Popup SortPopup;
        
        internal System.Windows.Controls.TextBlock PopupDateTitle;
        
        internal System.Windows.Controls.TextBlock PopupDateAsc;
        
        internal System.Windows.Controls.TextBlock PopupDateDesc;
        
        internal System.Windows.Controls.TextBlock PopupAirateAsc;
        
        internal System.Windows.Controls.TextBlock PopupAirdateDesc;
        
        internal System.Windows.Controls.TextBlock PopupTitleAsc;
        
        internal System.Windows.Controls.TextBlock PopupTitleDesc;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal Microsoft.Phone.Controls.PivotItem DefaultTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector DefaultRecordedLL;
        
        internal Microsoft.Phone.Controls.PivotItem DeletedTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector DeletedRecordedLL;
        
        internal Microsoft.Phone.Controls.PivotItem LiveTVTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector LiveTVRecordedLL;
        
        internal Microsoft.Phone.Controls.PivotItem AllTitle;
        
        internal Microsoft.Phone.Controls.LongListSelector AllRecordedLL;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbarRefresh;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbarSort;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/Recorded.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.SortPopup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("SortPopup")));
            this.PopupDateTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PopupDateTitle")));
            this.PopupDateAsc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupDateAsc")));
            this.PopupDateDesc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupDateDesc")));
            this.PopupAirateAsc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupAirateAsc")));
            this.PopupAirdateDesc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupAirdateDesc")));
            this.PopupTitleAsc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupTitleAsc")));
            this.PopupTitleDesc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupTitleDesc")));
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.DefaultTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("DefaultTitle")));
            this.DefaultRecordedLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("DefaultRecordedLL")));
            this.DeletedTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("DeletedTitle")));
            this.DeletedRecordedLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("DeletedRecordedLL")));
            this.LiveTVTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("LiveTVTitle")));
            this.LiveTVRecordedLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("LiveTVRecordedLL")));
            this.AllTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("AllTitle")));
            this.AllRecordedLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("AllRecordedLL")));
            this.appbarRefresh = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarRefresh")));
            this.appbarSort = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarSort")));
        }
    }
}

