﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\Guide.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "19B8C404C9D880A13EB7027ED7E0C82E"
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
    
    
    public partial class Guide : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Primitives.Popup SortPopup;
        
        internal System.Windows.Controls.TextBlock PopupDateTitle;
        
        internal System.Windows.Controls.TextBlock PopupChannelAsc;
        
        internal System.Windows.Controls.TextBlock PopupChannelDesc;
        
        internal System.Windows.Controls.TextBlock PopupRecstatusAsc;
        
        internal System.Windows.Controls.TextBlock PopupTitleAsc;
        
        internal System.Windows.Controls.TextBlock PopupTitleDesc;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal Microsoft.Phone.Controls.Pivot guidePivot;
        
        internal Microsoft.Phone.Controls.PivotItem nowPivot;
        
        internal Microsoft.Phone.Controls.LongListSelector NowGuideLL;
        
        internal Microsoft.Phone.Controls.PivotItem timePivot;
        
        internal Microsoft.Phone.Controls.DatePicker guideDatePicker;
        
        internal Microsoft.Phone.Controls.TimePicker guideTimePicker;
        
        internal System.Windows.Controls.Button guideTimePickerButton;
        
        internal Microsoft.Phone.Controls.LongListSelector TimeGuideLL;
        
        internal Microsoft.Phone.Controls.PivotItem channelPivot;
        
        internal Microsoft.Phone.Controls.ListPicker guideChannelPicker;
        
        internal Microsoft.Phone.Controls.DatePicker guideChannelDatePicker;
        
        internal System.Windows.Controls.Button guideChannelButton;
        
        internal Microsoft.Phone.Controls.LongListSelector ChannelGuideLL;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/Guide.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.SortPopup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("SortPopup")));
            this.PopupDateTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PopupDateTitle")));
            this.PopupChannelAsc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupChannelAsc")));
            this.PopupChannelDesc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupChannelDesc")));
            this.PopupRecstatusAsc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupRecstatusAsc")));
            this.PopupTitleAsc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupTitleAsc")));
            this.PopupTitleDesc = ((System.Windows.Controls.TextBlock)(this.FindName("PopupTitleDesc")));
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.guidePivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("guidePivot")));
            this.nowPivot = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("nowPivot")));
            this.NowGuideLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("NowGuideLL")));
            this.timePivot = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("timePivot")));
            this.guideDatePicker = ((Microsoft.Phone.Controls.DatePicker)(this.FindName("guideDatePicker")));
            this.guideTimePicker = ((Microsoft.Phone.Controls.TimePicker)(this.FindName("guideTimePicker")));
            this.guideTimePickerButton = ((System.Windows.Controls.Button)(this.FindName("guideTimePickerButton")));
            this.TimeGuideLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("TimeGuideLL")));
            this.channelPivot = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("channelPivot")));
            this.guideChannelPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("guideChannelPicker")));
            this.guideChannelDatePicker = ((Microsoft.Phone.Controls.DatePicker)(this.FindName("guideChannelDatePicker")));
            this.guideChannelButton = ((System.Windows.Controls.Button)(this.FindName("guideChannelButton")));
            this.ChannelGuideLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("ChannelGuideLL")));
            this.appbarRefresh = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarRefresh")));
            this.appbarSort = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarSort")));
        }
    }
}

