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
            XButton = new RelayCommand(o => Closing());
            folderWorker.DoWork += folderWorker_DoWork;
            folderWorker.RunWorkerCompleted += FolderWorker_RunWorkerCompleted;
            folderWorker.WorkerSupportsCancellation = true;
        }

        

        private Visibility _progressBarVisible = Visibility.Hidden;
        public Visibility ProgressBarVisible
        {
            get => _progressBarVisible;
            set
            {
                _progressBarVisible = value;
                OnPropertyChanged(nameof(ProgressBarVisible));
            }
        }

        public ICommand RefreshButton { get; private set; }
        private bool EnableRefreshButton { get; set; } = true;
        public ICommand OpenDirButton { get; private set; }
        private bool EnableOpenDirButton { get; set; } = true;
        public ICommand XButton { get; private set; }

        private BackgroundWorker folderWorker = new BackgroundWorker();

        private void folderWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var appdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfPlayer");
            var jsonFilename = Path.Combine(appdataDir, "Folders.json");

            var LoadedFolder = new Folder(LoadedDirectory);
            string json = JsonConvert.SerializeObject(LoadedFolder, Formatting.Indented);
            File.WriteAllText(jsonFilename, json);

            if (folderWorker.CancellationPending == true)
                e.Cancel = true;
            else
                e.Result = LoadedFolder;

            //((App)Application.Current).MWVM.LoadedFolder = LoadedFolder;
        }

        private void FolderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == false)
            {
                ((App)Application.Current).MWVM.LoadedFolder = (Folder)e.Result;
            }
            ProgressBarVisible = Visibility.Hidden;
        }

        private void Refresh()
        {
            ProgressBarVisible = Visibility.Visible;
            folderWorker.RunWorkerAsync();
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

        private void Closing()
        {
            if (folderWorker.IsBusy)
            {
                folderWorker.CancelAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
