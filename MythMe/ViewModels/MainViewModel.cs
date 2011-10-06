using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;


namespace MythMe
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Backends = new ObservableCollection<BackendsViewModel>();
            this.Frontends = new ObservableCollection<FrontendsViewModel>();

            this.Recorded = new ObservableCollection<ProgramViewModel>();
            this.Upcoming = new ObservableCollection<ProgramViewModel>();

            this.Channels = new ObservableCollection<ChannelViewModel>();

            this.SelectedProgram = new ProgramViewModel();

            this.appSettings = new AppSettings();

            this.functions = new FunctionsModel();

            this.Connected = false;

            this.prefs = IsolatedStorageSettings.ApplicationSettings;

        }

        /// <summary>
        /// A collection of objects.
        /// </summary>
        public ObservableCollection<BackendsViewModel> Backends { get; private set; }
        public ObservableCollection<FrontendsViewModel> Frontends { get; private set; }

        public ObservableCollection<ProgramViewModel> Recorded { get; private set; }
        public ObservableCollection<ProgramViewModel> Upcoming { get; private set; }

        public ObservableCollection<ChannelViewModel> Channels { get; private set; }

        public ProgramViewModel SelectedProgram;

        public string GuideTime { get; set; }

        public AppSettings appSettings;

        public FunctionsModel functions;

        public bool IsDataLoaded { get; private set; }

        public bool Connected { get; set; }

        private IsolatedStorageSettings prefs;

        public string randText()
        {
            //return random.Next().ToString();
            return myRandom();
        }
        private static string myRandom()
        {
            Random random = new Random();

            return random.Next().ToString();
        }

        /// <summary>
        /// Creates and adds a few objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            
            //load backends
            var savedBackendsList = StorageLoad<List<BackendsViewModel>>("Backends");

            if (savedBackendsList.Count < 1)
            {
                this.Backends.Add(new BackendsViewModel() { Name = "wes-htpc", Host = "192.168.1.105", ProtoPort = 6543, XmlPort = 6544, Master = true });
                this.Backends.Add(new BackendsViewModel() { Name = "wes-desktop", Host = "192.168.1.110", ProtoPort = 6543, XmlPort = 6544, Master = false });
            }
            else
            {
                foreach (var e in savedBackendsList) this.Backends.Add(e);

            }

            //load frontends
            var savedFrontendsList = StorageLoad<List<FrontendsViewModel>>("Frontends");

            if (savedFrontendsList.Count < 1)
            {
                this.Frontends.Add(new FrontendsViewModel() { Name = "wes-ion", Host = "192.168.1.104", Port = 6546 });
            }
            else
            {
                foreach (var e in savedFrontendsList) this.Frontends.Add(e);

            }

            this.GuideTime = "none";

            //save hosts
            this.saveBackends();
            this.saveFrontends();

            //load prefs
            //appSettings.Save();

            this.IsDataLoaded = true;
        }


        public void savePrefs()
        {
            appSettings.Save();
        }


        public void saveBackends()
        {
            List<BackendsViewModel> hostsList = new List<BackendsViewModel>(this.Backends);
            StorageSave<List<BackendsViewModel>>("Backends", hostsList);
        }
        public void deleteBackend(BackendsViewModel inHost)
        {
            this.Backends.Remove(inHost);

            this.saveBackends();
        }

        public void saveFrontends()
        {
            List<FrontendsViewModel> hostsList = new List<FrontendsViewModel>(this.Frontends);
            StorageSave<List<FrontendsViewModel>>("Frontends", hostsList);
        }
        public void deleteFrontend(FrontendsViewModel inHost)
        {
            this.Frontends.Remove(inHost);

            this.saveFrontends();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public static T StorageLoad<T>(string name) where T : class, new()
        {
            T loadedObject = null;
            using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication())
            using (IsolatedStorageFileStream storageFileStream = new IsolatedStorageFileStream(name, System.IO.FileMode.OpenOrCreate, storageFile))
            {
                if (storageFileStream.Length > 0)
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    loadedObject = serializer.ReadObject(storageFileStream) as T;
                }
                if (loadedObject == null)
                {
                    loadedObject = new T();
                }
            }

            return loadedObject;
        }
        public static void StorageSave<T>(string name, T objectToSave)
        {
            using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication())
            using (IsolatedStorageFileStream storageFileStream = new IsolatedStorageFileStream(name, System.IO.FileMode.Create, storageFile))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(storageFileStream, objectToSave);
            }
        }
        public static void StorageDelete(string name)
        {
            using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storageFile.Remove();
            }
        }




    }
}