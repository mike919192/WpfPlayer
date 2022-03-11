using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfPlayer.Model;

namespace WpfPlayer.ViewModel
{
    class OptionsViewModel : INotifyPropertyChanged
    {
        private string _loadedDirectory;

        public string LoadedDirectory
        {
            get => _loadedDirectory;
            set
            {
                _loadedDirectory = value;
                Properties.Settings.Default.LoadedDirectory = value;
                OnPropertyChanged(nameof(LoadedDirectory));
            }
        }

        public OptionsViewModel()
        {
            RefreshButton = new RelayCommand(o => Refresh(), o => EnableRefreshButton);
            OpenDirButton = new RelayCommand(o => OpenDir(), o => EnableOpenDirButton);
        }

        public ICommand RefreshButton { get; private set; }
        private bool EnableRefreshButton { get; set; } = true;
        public ICommand OpenDirButton { get; private set; }
        private bool EnableOpenDirButton { get; set; } = true;

        private void Refresh()
        {
            var appdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfPlayer");
            var jsonFilename = Path.Combine(appdataDir, "Folders.json");

            var LoadedFolder = new Folder(LoadedDirectory);
            string json = JsonConvert.SerializeObject(LoadedFolder, Formatting.Indented);
            File.WriteAllText(jsonFilename, json);

            ((App)Application.Current).MWVM.LoadedFolder = LoadedFolder;
        }
        private void OpenDir()
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    LoadedDirectory = fbd.SelectedPath;

                    Refresh();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
