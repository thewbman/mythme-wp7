﻿#pragma checksum "C:\Users\wes\SVN\MythMe\MythMe\Remote.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "CC6A68F989DB2677ACEB5190F3FF0641"
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
    
    
    public partial class Remote : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.Pivot remoteTitle;
        
        internal System.Windows.Controls.TextBlock escape;
        
        internal System.Windows.Controls.TextBlock up;
        
        internal System.Windows.Controls.TextBlock delete;
        
        internal System.Windows.Controls.TextBlock left;
        
        internal System.Windows.Controls.TextBlock enter;
        
        internal System.Windows.Controls.TextBlock right;
        
        internal System.Windows.Controls.TextBlock menu;
        
        internal System.Windows.Controls.TextBlock down;
        
        internal System.Windows.Controls.TextBlock info;
        
        internal System.Windows.Controls.TextBlock previous;
        
        internal System.Windows.Controls.TextBlock pause;
        
        internal System.Windows.Controls.TextBlock next;
        
        internal System.Windows.Controls.TextBlock livetv;
        
        internal System.Windows.Controls.TextBlock recorded;
        
        internal System.Windows.Controls.TextBlock video;
        
        internal System.Windows.Controls.TextBlock music;
        
        internal System.Windows.Controls.TextBlock location;
        
        internal System.Windows.Controls.TextBlock volume;
        
        internal System.Windows.Controls.TextBlock remoteResponse;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MythMe;component/Remote.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.remoteTitle = ((Microsoft.Phone.Controls.Pivot)(this.FindName("remoteTitle")));
            this.escape = ((System.Windows.Controls.TextBlock)(this.FindName("escape")));
            this.up = ((System.Windows.Controls.TextBlock)(this.FindName("up")));
            this.delete = ((System.Windows.Controls.TextBlock)(this.FindName("delete")));
            this.left = ((System.Windows.Controls.TextBlock)(this.FindName("left")));
            this.enter = ((System.Windows.Controls.TextBlock)(this.FindName("enter")));
            this.right = ((System.Windows.Controls.TextBlock)(this.FindName("right")));
            this.menu = ((System.Windows.Controls.TextBlock)(this.FindName("menu")));
            this.down = ((System.Windows.Controls.TextBlock)(this.FindName("down")));
            this.info = ((System.Windows.Controls.TextBlock)(this.FindName("info")));
            this.previous = ((System.Windows.Controls.TextBlock)(this.FindName("previous")));
            this.pause = ((System.Windows.Controls.TextBlock)(this.FindName("pause")));
            this.next = ((System.Windows.Controls.TextBlock)(this.FindName("next")));
            this.livetv = ((System.Windows.Controls.TextBlock)(this.FindName("livetv")));
            this.recorded = ((System.Windows.Controls.TextBlock)(this.FindName("recorded")));
            this.video = ((System.Windows.Controls.TextBlock)(this.FindName("video")));
            this.music = ((System.Windows.Controls.TextBlock)(this.FindName("music")));
            this.location = ((System.Windows.Controls.TextBlock)(this.FindName("location")));
            this.volume = ((System.Windows.Controls.TextBlock)(this.FindName("volume")));
            this.remoteResponse = ((System.Windows.Controls.TextBlock)(this.FindName("remoteResponse")));
        }
    }
}

