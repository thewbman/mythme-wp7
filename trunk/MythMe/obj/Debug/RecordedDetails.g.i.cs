﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\RecordedDetails.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5D502400019614BB2A349FA94B9C1ADB"
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
    
    
    public partial class RecordedDetails : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.Panorama topPanorama;
        
        internal System.Windows.Controls.TextBlock playButton;
        
        internal System.Windows.Controls.TextBlock webButton;
        
        internal System.Windows.Controls.TextBlock deleteButton;
        
        internal System.Windows.Controls.TextBlock undeleteButton;
        
        internal System.Windows.Controls.TextBlock jobsButton;
        
        internal System.Windows.Controls.TextBlock scheduleButton;
        
        internal System.Windows.Controls.TextBlock guideButton;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/RecordedDetails.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.topPanorama = ((Microsoft.Phone.Controls.Panorama)(this.FindName("topPanorama")));
            this.playButton = ((System.Windows.Controls.TextBlock)(this.FindName("playButton")));
            this.webButton = ((System.Windows.Controls.TextBlock)(this.FindName("webButton")));
            this.deleteButton = ((System.Windows.Controls.TextBlock)(this.FindName("deleteButton")));
            this.undeleteButton = ((System.Windows.Controls.TextBlock)(this.FindName("undeleteButton")));
            this.jobsButton = ((System.Windows.Controls.TextBlock)(this.FindName("jobsButton")));
            this.scheduleButton = ((System.Windows.Controls.TextBlock)(this.FindName("scheduleButton")));
            this.guideButton = ((System.Windows.Controls.TextBlock)(this.FindName("guideButton")));
        }
    }
}

