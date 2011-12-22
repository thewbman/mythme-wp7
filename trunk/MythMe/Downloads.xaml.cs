using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;

namespace MythMe
{
    public partial class Downloads : PhoneApplicationPage
    {
        public Downloads()
        {
            InitializeComponent();

            List<NameContentViewModel> allFiles = new List<NameContentViewModel>();

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/shared/transfers"))
                {
                    isoStore.CreateDirectory("/shared/transfers");
                }
            }
        }

        private List<NameContentViewModel> allFiles;
        private List<NameContentViewModel> listFiles;
        private List<NameContentViewModel> inprogressTransfers;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            RefreshData();

        }

        private void RefreshData()
        {
            
            allFiles = new List<NameContentViewModel>();
            listFiles = new List<NameContentViewModel>();
            inprogressTransfers = new List<NameContentViewModel>();

            //MessageBox.Show("There are "+BackgroundTransferService.Requests.Count().ToString() + " current downloads");
            
            foreach (BackgroundTransferRequest b in BackgroundTransferService.Requests)
            {
                if (b.TransferStatus == TransferStatus.Completed)
                {
                    BackgroundTransferService.Remove(b);
                }
                else
                {

                    NameContentViewModel n = new NameContentViewModel();

                    n.Content = b.TransferStatus.ToString();
                    n.Name = b.Tag;
                    n.First = b.RequestUri.ToString();
                    n.Second = b.DownloadLocation.ToString();
                    n.Third = b.TotalBytesToReceive.ToString() + " bytes to receive";

                    inprogressTransfers.Add(n);
                }
            }

            inprogressList.ItemsSource = inprogressTransfers;


            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                
                    //string searchpath = System.IO.Path.Combine("shared/transfers/", "*.*");
                    string searchpath = System.IO.Path.Combine("Shared\\Transfers", "*.*");
                    //searchpath = string.Format("{0}\\*.*", searchpath);

                    //foreach (string s in isoStore.GetFileNames())
                    foreach (string s in isoStore.GetFileNames(searchpath))
                    {
                        NameContentViewModel n = new NameContentViewModel();

                        n.Content = s;
                        n.Name = "file";

                        allFiles.Add(n);
                    }


            }


            foreach (NameContentViewModel file in allFiles)
            {
                bool isActive = false;

                foreach (NameContentViewModel transfers in inprogressTransfers)
                {
                    if (transfers.Second.Contains(file.Content))    // .Replace("\\shared\\transfers\\",""))
                        isActive = true;
                }

                if (isActive)
                {
                    //dont show
                }
                else
                {
                    NameContentViewModel s = new NameContentViewModel();

                    string[] stringSeparators = new string[] { "___" };
                    string[] filesplit = file.Content.Split(stringSeparators, StringSplitOptions.None);

                    if (filesplit.Count() == 4)
                    {
                        s.Content = file.Content;

                        s.First = filesplit[0].Replace("_", ":"); //starttime
                        s.Second = filesplit[1];    //chanid
                        s.Third = Uri.UnescapeDataString(filesplit[2]); //title
                        s.Fourth = Uri.UnescapeDataString(filesplit[3].Replace(".mp4", "")); //subtitle

                        s.Name = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + s.Second;
                    }
                    else if (filesplit.Count() == 3)
                    {
                        s.Content = file.Content;

                        s.First = filesplit[0].Replace("_", ":"); //starttime
                        s.Second = filesplit[1];    //chanid
                        s.Third = Uri.UnescapeDataString(filesplit[2].Replace(".mp4", "")); ; //title
                        //s.Fourth = filesplit[3].Replace(".mp4", ""); //subtitle

                        s.Name = "http://" + App.ViewModel.appSettings.MasterBackendIpSetting + ":" + App.ViewModel.appSettings.MasterBackendXmlPortSetting + "/Myth/GetChannelIcon?ChanId=" + s.Second;
                    }
                    else
                    {
                        s.Content = file.Content;
                        s.Third = file.Content;
                    }

                    listFiles.Add(s);
                }
            }

            completedList.ItemsSource = listFiles;
                
        }

        private void completedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (completedList.SelectedItem == null)
                return;

            NameContentViewModel s = (NameContentViewModel)completedList.SelectedItem;


            MediaPlayerLauncher m = new MediaPlayerLauncher();
            m.Media = new Uri("shared/transfers/" + s.Content, UriKind.RelativeOrAbsolute);
            m.Orientation = MediaPlayerOrientation.Landscape;
            m.Location = MediaLocationType.Data;
            m.Show();

            completedList.SelectedItem = null;
        }

        private void deleteFile_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            NameContentViewModel selectedItem = menu.DataContext as NameContentViewModel;

            if (selectedItem == null)
                return;

            if (MessageBox.Show("Are you sure you want to delete this file?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string path = "shared/transfers/" + selectedItem.Content;

                    if (isoStore.FileExists(path))
                    {
                        isoStore.DeleteFile(path);

                        //MessageBox.Show("Successfully deleted.");

                        RefreshData();
                    }
                    else
                    {
                        MessageBox.Show("Could not delete file: " + path);
                    }
                }
             
            }
        }

        private void playFile_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            NameContentViewModel selectedItem = menu.DataContext as NameContentViewModel;

            if (selectedItem == null)
                return;

            NameContentViewModel s = selectedItem;


            MediaPlayerLauncher m = new MediaPlayerLauncher();
            m.Media = new Uri("shared/transfers/" + s.Content, UriKind.RelativeOrAbsolute);
            m.Orientation = MediaPlayerOrientation.Landscape;
            m.Location = MediaLocationType.Data;
            m.Show();

            completedList.SelectedItem = null;
        }

        private void appbarRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }
    }
}