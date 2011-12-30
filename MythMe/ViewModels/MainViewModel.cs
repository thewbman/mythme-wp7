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
            this.Backends = new ObservableCollection<BackendViewModel>();
            this.Frontends = new ObservableCollection<FrontendViewModel>();

            this.Recorded = new ObservableCollection<ProgramViewModel>();
            this.Upcoming = new ObservableCollection<ProgramViewModel>();

            this.Channels = new ObservableCollection<ChannelViewModel>();

            this.SelectedRecordedProgram = new ProgramViewModel();
            this.SelectedUpcomingProgram = new ProgramViewModel();
            this.SelectedGuideProgram = new ProgramViewModel();
            this.SelectedSearchProgram = new ProgramViewModel();
            this.SelectedPeopleProgram = new ProgramViewModel();

            this.SelectedPerson = new PeopleViewModel();

            this.appSettings = new AppSettings();

            this.functions = new FunctionsModel();

            this.encoder = new UTF8Encoding();

            this.Connected = false;

            this.prefs = IsolatedStorageSettings.ApplicationSettings;

        }

        /// <summary>
        /// A collection of objects.
        /// </summary>
        public ObservableCollection<BackendViewModel> Backends { get; private set; }
        public ObservableCollection<FrontendViewModel> Frontends { get; private set; }

        public ObservableCollection<ProgramViewModel> Recorded { get; private set; }
        public ObservableCollection<ProgramViewModel> Upcoming { get; private set; }

        public ObservableCollection<ChannelViewModel> Channels { get; private set; }

        public ProgramViewModel SelectedRecordedProgram;
        public ProgramViewModel SelectedUpcomingProgram;
        public ProgramViewModel SelectedGuideProgram;
        public ProgramViewModel SelectedSearchProgram;
        public ProgramViewModel SelectedPeopleProgram;

        public PeopleViewModel SelectedPerson;

        public string GuideTime { get; set; }

        public AppSettings appSettings;

        public FunctionsModel functions;

        public UTF8Encoding encoder;

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
            var savedBackendsList = StorageLoad<List<BackendViewModel>>("Backends");

            if (savedBackendsList.Count < 1)
            {
                //this.Backends.Add(new BackendViewModel() { Name = "wes-htpc", Host = "192.168.1.105", ProtoPort = 6543, XmlPort = 6544, Master = true });
                //this.Backends.Add(new BackendViewModel() { Name = "wes-desktop", Host = "192.168.1.110", ProtoPort = 6543, XmlPort = 6544, Master = false });
            }
            else
            {
                foreach (var e in savedBackendsList) this.Backends.Add(e);

            }

            //load frontends
            var savedFrontendsList = StorageLoad<List<FrontendViewModel>>("Frontends");

            if (savedFrontendsList.Count < 1)
            {
                //this.Frontends.Add(new FrontendViewModel() { Name = "wes-ion", Address = "192.168.1.104", Port = 6546 });
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
            List<BackendViewModel> hostsList = new List<BackendViewModel>(this.Backends);
            StorageSave<List<BackendViewModel>>("Backends", hostsList);
        }
        public void deleteBackend(BackendViewModel inHost)
        {
            this.Backends.Remove(inHost);

            this.saveBackends();
        }

        public void saveFrontends()
        {
            List<FrontendViewModel> hostsList = new List<FrontendViewModel>(this.Frontends);
            StorageSave<List<FrontendViewModel>>("Frontends", hostsList);
        }
        public void deleteFrontend(FrontendViewModel inHost)
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