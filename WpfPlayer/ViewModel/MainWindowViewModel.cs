using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfPlayer.Model;

namespace WpfPlayer.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _positionSliderMouseDown = false;
        public object SelectedTreeViewItem { get; set; }

        private List<string> _playlist = new List<string>();
        public List<string> Playlist
        {
            get
            {
                List<string> friendlyPlaylist = new List<string>();
                foreach(var filePath in _playlist)
                {
                    friendlyPlaylist.Add(Path.GetFileNameWithoutExtension(filePath));
                }
                return friendlyPlaylist;
            }
        }

        private int _playlistPosition = 0;
        public int PlaylistPosition
        {
            get => _playlistPosition;
            set
            {
                _playlistPosition = value;
                OnPropertyChanged(nameof(PlaylistPosition));
            }
        }
        private string _loadedDir;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        
        

        private string _playPauseIcon;
        public string PlayPauseIcon
        {
            get => _playPauseIcon;
            set
            {
                _playPauseIcon = value;
                OnPropertyChanged(nameof(PlayPauseIcon));
            }
        }

        public string RewIcon
        {
            get => new string("pack://application:,,,/WpfPlayer;component/resources/rew.png");
        }

        public string FFIcon
        {
            get => new string("pack://application:,,,/WpfPlayer;component/resources/ff.png");
        }

        public string VolumeIcon
        {
            get => new string("pack://application:,,,/WpfPlayer;component/resources/volume.png");
        }

        private double _volumeValue;
        public double VolumeValue
        {
            get => _volumeValue;
            set
            {
                _volumeValue = value;
                musicEngine.Volume = _volumeValue;
                OnPropertyChanged(nameof(VolumeValue));
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        private double _progressControlValue;
        public double ProgressControlValue
        {
            get => _progressControlValue;
            set
            {
                _progressControlValue = value;
                OnPropertyChanged(nameof(ProgressControlValue));
            }
        }

        public TimeSpan _timeRemaining;
        public string TimeRemaining
        {
            get
            {
                return "-" + _timeRemaining.Minutes + ":" + _timeRemaining.Seconds;
            }
        }

        private Uri _displayedImagePath;
        public Uri DisplayedImagePath
        {
            get
            {
                if (_displayedImagePath == null)
                    return new Uri("pack://application:,,,/WpfPlayer;component/resources/Music-icon-512.png");
                else
                    return _displayedImagePath;
            }
            set
            {
                if (_displayedImagePath != value)
                {
                    _displayedImagePath = value;
                    OnPropertyChanged(nameof(DisplayedImagePath));
                }
            }
        }

        private string _artistString;
        public string ArtistString
        {
            get => _artistString;
            set
            {
                _artistString = value;
                OnPropertyChanged(nameof(ArtistString));
            }
        }

        private string _albumString;
        public string AlbumString
        {
            get => _albumString;
            set
            {
                _albumString = value;
                OnPropertyChanged(nameof(AlbumString));
            }
        }

        private string _songTitleString;
        public string SongTitleString
        {
            get => _songTitleString;
            set
            {
                _songTitleString = value;
                OnPropertyChanged(nameof(SongTitleString));
            }
        }

        private Folder _loadedFolder;
        public Folder LoadedFolder
        {
            get => _loadedFolder;
            set
            {
                _loadedFolder = value;
                OnPropertyChanged(nameof(LoadedFolder));
            }
        }

        private MusicEngine musicEngine = new MusicEngine();

        public MainWindowViewModel()
        {
            PlayButton = new RelayCommand(o => PlayPause(), o => EnablePlayButton);
            RewButton = new RelayCommand(o => Rew(), o => EnableRewButton);
            FFButton = new RelayCommand(o => FF(), o => EnableFFButton);
            SetPositionButton = new RelayCommand(o => SetPosition());
            ProgressControlButton = new RelayCommand(o => ProgressControlButtonMouseDown());
            LoadDirButton = new RelayCommand(o => LoadDir(), o => EnableLoadDirButton);
            XButton = new RelayCommand(o => Closing());
            PlayPauseIcon = "pack://application:,,,/WpfPlayer;component/resources/play.png";
            VolumeValue = Properties.Settings.Default.Volume;
            ProgressValue = 0.0;

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            musicEngine.SongStarted += MusicEngine_SongStarted;
            musicEngine.SongFinished += MusicEngine_SongFinished;
            musicEngine.PlaylistFinished += MusicEngine_PlaylistFinished;

            LoadedFolder = new Folder(@"\\192.168.1.145\JMicron\MUSIC");
        }

        private void MusicEngine_PlaylistFinished(object sender, EventArgs e)
        {
            if (dispatcherTimer.IsEnabled == true)
                dispatcherTimer.Stop();
            PlaylistPosition = 0;
            ProgressControlValue = 0.0;
            ProgressValue = 0.0;
            PlayPauseIcon = "pack://application:,,,/WpfPlayer;component/resources/play.png";
        }

        private void MusicEngine_SongFinished(object sender, EventArgs e)
        {
            if (PlaylistPosition == _playlist.Count - 2)
            {
                PlaylistPosition++;
                musicEngine.Play(_playlist[PlaylistPosition], null);
            }
            else
            {
                PlaylistPosition++;
                musicEngine.Play(_playlist[PlaylistPosition], _playlist[PlaylistPosition + 1]);
            }
        }

        private void MusicEngine_SongStarted(object sender, SongStartedEventArgs e)
        {
            ArtistString = musicEngine.ArtistString;
            AlbumString = musicEngine.AlbumString;
            SongTitleString = musicEngine.SongTitleString;

            if (File.Exists(e.Path + @"\folder.jpg"))
                DisplayedImagePath = new Uri(e.Path + @"\folder.jpg");
            else if (File.Exists(e.Path + @"\folder.jpeg"))
                DisplayedImagePath = new Uri(e.Path + @"\folder.jpeg");
            else if (File.Exists(e.Path + @"\cover.jpg"))
                DisplayedImagePath = new Uri(e.Path + @"\cover.jpg");
            else
                DisplayedImagePath = null;
        }

        //Sets up Buttons
        public ICommand PlayButton { get; private set; }
        private bool EnablePlayButton { get; set; } = true;
        public ICommand RewButton { get; private set; }
        private bool EnableRewButton { get; set; } = true;
        public ICommand FFButton { get; private set; }
        private bool EnableFFButton { get; set; } = true;
        public ICommand ProgressControlButton { get; private set; }
        public ICommand SetPositionButton { get; private set; }
        public ICommand LoadDirButton { get; private set; }
        private bool EnableLoadDirButton { get; set; } = true;
        public ICommand XButton { get; private set; }

        private void Closing()
        {
            musicEngine.Stop();
            
            Properties.Settings.Default.Volume = VolumeValue;
            Properties.Settings.Default.Save();
        }

        private void PlayPause()
        {
            if (musicEngine.IsPlaying == false)
            {
                if (_playlist.Count == 1)
                    musicEngine.Play(_playlist[_playlistPosition], null);
                else
                    musicEngine.Play(_playlist[_playlistPosition], _playlist[_playlistPosition + 1]);
                if (dispatcherTimer.IsEnabled == false)
                    dispatcherTimer.Start();
                PlayPauseIcon = "pack://application:,,,/WpfPlayer;component/resources/pause.png";
            }
            else
            {
                musicEngine.Pause();
                if (dispatcherTimer.IsEnabled == true)
                    dispatcherTimer.Stop();
                PlayPauseIcon = "pack://application:,,,/WpfPlayer;component/resources/play.png";
            }            
        }

        private void SetPosition()
        {
            musicEngine.Position = ProgressControlValue;
            ProgressValue = ProgressControlValue;
            _positionSliderMouseDown = false;
        }

        private void LoadDir()
        {
            //using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            //{
            //System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            //if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            //{
            //LoadedFolder = new Folder(fbd.SelectedPath);

            _loadedDir = Path.Combine(((Folder)SelectedTreeViewItem).Path, ((Folder)SelectedTreeViewItem).FolderName);

            if (musicEngine.IsPlaying || musicEngine.IsPaused)
            {
                _playlist.Clear();
                _playlistPosition = 0;
                musicEngine.Stop();
            }
            //_loadedDir = fbd.SelectedPath;
            _playlist = getFiles(_loadedDir, "*.mp3|*.flac", SearchOption.AllDirectories).ToList();
            OnPropertyChanged(nameof(Playlist));            

            PlayPause();


                //}
            //}
        }        

        private void Rew()
        {
            musicEngine.Rew();
            ProgressValue = 0.0;
            ProgressControlValue = 0.0;
        }

        private void FF()
        {
            musicEngine.FF();
            ProgressValue = 0.0;
            ProgressControlValue = 0.0;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressValue = musicEngine.Position;
                if (_positionSliderMouseDown == false)
                    ProgressControlValue = ProgressValue;
                _timeRemaining = musicEngine.TimeRemaining;
                OnPropertyChanged(nameof(TimeRemaining));
            });
        }        

        private void ProgressControlButtonMouseDown()
        {
            _positionSliderMouseDown = true;
        }

        public string[] getFiles(string SourceFolder, string Filter, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // ArrayList will hold all file names
            ArrayList alFiles = new ArrayList();

            // Create an array of filter string
            string[] MultipleFilters = Filter.Split('|');

            // for each filter find mathing file names
            foreach (string FileFilter in MultipleFilters)
            {
                // add found file names to array list
                alFiles.AddRange(Directory.GetFiles(SourceFolder, FileFilter, searchOption));
            }

            // returns string array of relevant file names
            return (string[])alFiles.ToArray(typeof(string));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExtendedTreeView : TreeView
    {
        public ExtendedTreeView()
            : base()
        {
            this.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(___ICH);
        }

        void ___ICH(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem != null)
            {
                SetValue(SelectedItem_Property, SelectedItem);
            }
        }

        public object SelectedItem_
        {
            get { return (object)GetValue(SelectedItem_Property); }
            set { SetValue(SelectedItem_Property, value); }
        }
        public static readonly DependencyProperty SelectedItem_Property = DependencyProperty.Register("SelectedItem_", typeof(object), typeof(ExtendedTreeView), new UIPropertyMetadata(null));
    }
}
