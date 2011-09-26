using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls;

namespace MythMe
{
    public partial class RecordedDetails : PhoneApplicationPage
    {
        public RecordedDetails()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedProgram;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //text1.Text = App.ViewModel.SelectedProgram.description;

            BitmapImage bitmapImage = new BitmapImage(new Uri(App.ViewModel.SelectedProgram.screenshot));
            //panoramaBackground.ImageSource = bitmapImage;
        }

        private void scheduleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DateTime dateResult;
            DateTime.TryParse(App.ViewModel.SelectedProgram.recstartts, out dateResult);

            //TimeSpan s = (DateTime.Now - new DateTime(1970, 1, 1, ));
            TimeSpan t = (dateResult - new DateTime(1970, 1, 1));
            //TimeSpan u = (dateResult - DateTime.Now);
            Int64 timestamp = (Int64)t.TotalSeconds - (Int64)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
            //Int64 timestamp = (Int64)s.TotalSeconds + (Int64)u.TotalSeconds;

            WebBrowserTask webopen = new WebBrowserTask();

            webopen.Uri = new Uri("http://" + App.ViewModel.appSettings.MasterBackendIpSetting + "/mythweb/tv/detail/" + App.ViewModel.SelectedProgram.chanid + "/" + timestamp);
            webopen.Show();
        }

    }
}