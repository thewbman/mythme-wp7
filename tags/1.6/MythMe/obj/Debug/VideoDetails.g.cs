﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\VideoDetails.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "17F356BF92857C13BF810485C3E6C760"
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
    
    
    public partial class VideoDetails : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.Panorama topPanorama;
        
        internal Microsoft.Phone.Controls.PanoramaItem peoplePivot;
        
        internal System.Windows.Controls.ListBox peopleList;
        
        internal System.Windows.Controls.Button playButton;
        
        internal System.Windows.Controls.Button homepageButton;
        
        internal System.Windows.Controls.Button titleSearchButton;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/VideoDetails.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.topPanorama = ((Microsoft.Phone.Controls.Panorama)(this.FindName("topPanorama")));
            this.peoplePivot = ((Microsoft.Phone.Controls.PanoramaItem)(this.FindName("peoplePivot")));
            this.peopleList = ((System.Windows.Controls.ListBox)(this.FindName("peopleList")));
            this.playButton = ((System.Windows.Controls.Button)(this.FindName("playButton")));
            this.homepageButton = ((System.Windows.Controls.Button)(this.FindName("homepageButton")));
            this.titleSearchButton = ((System.Windows.Controls.Button)(this.FindName("titleSearchButton")));
        }
    }
}

