﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\Status.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "430F5025DBF5A2AF011136542EFF7F3F"
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
    
    
    public partial class Status : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal System.Windows.Controls.ListBox EncodersListBox;
        
        internal System.Windows.Controls.ListBox ScheduledListBox;
        
        internal System.Windows.Controls.ListBox JobqueueListBox;
        
        internal System.Windows.Controls.ListBox StorageListBox;
        
        internal System.Windows.Controls.ListBox GuideListBox;
        
        internal System.Windows.Controls.ListBox OtherListBox;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/Status.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.EncodersListBox = ((System.Windows.Controls.ListBox)(this.FindName("EncodersListBox")));
            this.ScheduledListBox = ((System.Windows.Controls.ListBox)(this.FindName("ScheduledListBox")));
            this.JobqueueListBox = ((System.Windows.Controls.ListBox)(this.FindName("JobqueueListBox")));
            this.StorageListBox = ((System.Windows.Controls.ListBox)(this.FindName("StorageListBox")));
            this.GuideListBox = ((System.Windows.Controls.ListBox)(this.FindName("GuideListBox")));
            this.OtherListBox = ((System.Windows.Controls.ListBox)(this.FindName("OtherListBox")));
            this.appbarRefresh = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarRefresh")));
        }
    }
}

