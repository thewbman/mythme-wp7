﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\Recorded.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "18613A5A04349D07221C94EBAEE432D1"
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
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal Microsoft.Phone.Controls.PivotItem DefaultTitle;
        
        internal Microsoft.Phone.Controls.ContextMenu SortPopup;
        
        internal System.Windows.Controls.ListBox DefaultRecordedListBox;
        
        internal Microsoft.Phone.Controls.PivotItem DeletedTitle;
        
        internal System.Windows.Controls.ListBox DeletedRecordedListBox;
        
        internal Microsoft.Phone.Controls.PivotItem LiveTVTitle;
        
        internal System.Windows.Controls.ListBox LiveTVRecordedListBox;
        
        internal Microsoft.Phone.Controls.PivotItem AllTitle;
        
        internal System.Windows.Controls.ListBox AllRecordedListBox;
        
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
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.DefaultTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("DefaultTitle")));
            this.SortPopup = ((Microsoft.Phone.Controls.ContextMenu)(this.FindName("SortPopup")));
            this.DefaultRecordedListBox = ((System.Windows.Controls.ListBox)(this.FindName("DefaultRecordedListBox")));
            this.DeletedTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("DeletedTitle")));
            this.DeletedRecordedListBox = ((System.Windows.Controls.ListBox)(this.FindName("DeletedRecordedListBox")));
            this.LiveTVTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("LiveTVTitle")));
            this.LiveTVRecordedListBox = ((System.Windows.Controls.ListBox)(this.FindName("LiveTVRecordedListBox")));
            this.AllTitle = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("AllTitle")));
            this.AllRecordedListBox = ((System.Windows.Controls.ListBox)(this.FindName("AllRecordedListBox")));
            this.appbarRefresh = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarRefresh")));
            this.appbarSort = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarSort")));
        }
    }
}

