﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\People.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C3A62A1AD4CC84057A4EE7E4669C40C1"
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
    
    
    public partial class People : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.TextBox searchBox;
        
        internal System.Windows.Controls.Button searchBoxButton;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar performanceProgressBarCustomized;
        
        internal Microsoft.Phone.Controls.Pivot PeoplePivot;
        
        internal Microsoft.Phone.Controls.LongListSelector PeopleLL;
        
        internal Microsoft.Phone.Controls.LongListSelector ProgramsLL;
        
        internal Microsoft.Phone.Controls.LongListSelector VideosLL;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbarSearch;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/People.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.searchBox = ((System.Windows.Controls.TextBox)(this.FindName("searchBox")));
            this.searchBoxButton = ((System.Windows.Controls.Button)(this.FindName("searchBoxButton")));
            this.performanceProgressBarCustomized = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("performanceProgressBarCustomized")));
            this.PeoplePivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("PeoplePivot")));
            this.PeopleLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("PeopleLL")));
            this.ProgramsLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("ProgramsLL")));
            this.VideosLL = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("VideosLL")));
            this.appbarSearch = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarSearch")));
        }
    }
}
