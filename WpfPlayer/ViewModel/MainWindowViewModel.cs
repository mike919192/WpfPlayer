using Newtonsoft.Json;
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
using WpfPlayer.View;

namespace WpfPlayer.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _positionSliderMouseDown = false;
        public object SelectedTreeViewItem { get; set; }

        private List<int> _playOrder = new List<int>();

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

        private int _selectedPlaylistPosition = 0;
        private int _playlistPosition = 0;
        public int SelectedPlaylistPosition
        {
            get => _selectedPlaylistPosition;
            set
            {
                _selectedPlaylistPosition = value;
                OnPropertyChanged(nameof(SelectedPlaylistPosition));
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

        private string _playPauseWhiteIcon;
        public string PlayPauseWhiteIcon
        {
            get => _playPauseWhiteIcon;
            set
            {
                _playPauseWhiteIcon = value;
                OnPropertyChanged(nameof(PlayPauseWhiteIcon));
            }
        }

        private ShuffleEnum _shuffleState;
        private string _shuffleIcon;
        public string ShuffleIcon
        {
            get => _shuffleIcon;
            set
            {
                _shuffleIcon = value;
                OnPropertyChanged(nameof(ShuffleIcon));
            }
        }

        private void SetPlayPauseIcon(PlayPauseEnum playPauseIn)
        {
            if (playPauseIn == PlayPauseEnum.Play)
            {
                PlayPauseIcon = "pack://application:,,,/WpfPlayer;component/resources/play.png";
                PlayPauseWhiteIcon = "pack://application:,,,/WpfPlayer;component/resources/play_white.png";
            }
            else
            {
                PlayPauseIcon = "pack://application:,,,/WpfPlayer;component/resources/pause.png";
                PlayPauseWhiteIcon = "pack://application:,,,/WpfPlayer;component/resources/pause_white.png";
            }
        }

        private void SetShuffleIcon(ShuffleEnum shuffleIn)
        {
            if (shuffleIn == ShuffleEnum.On)
            {
                ShuffleIcon = "pack://application:,,,/WpfPlayer;component/resources/shuffle_active.png";
            }
            else
            {
                ShuffleIcon = "pack://application:,,,/WpfPlayer;component/resources/shuffle.png";
            }
        }

        private enum PlayPauseEnum
        {
            Play, Pause
        }

        private enum ShuffleEnum
        {
            On, Off
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
                return "-" + _timeRemaining.ToString(@"m\:ss");
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

                    ThumbnailChangedEventArgs args = new ThumbnailChangedEventArgs
                    {
                        Path = _displayedImagePath?.LocalPath
                    };
                    OnThumbnailChanged(args);
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
            PlayTrackButton = new RelayCommand(o => PlayTrack(), o => EnablePlayTrackButton);
            ShuffleButton = new RelayCommand(o => ToggleShuffle(), o => EnableShuffleButton);
            OptionsButton = new RelayCommand(o => OpenOptions(), o => EnableOptionsButton);
            XButton = new RelayCommand(o => Closing());
            
            SetPlayPauseIcon(PlayPauseEnum.Play);
            _shuffleState = Properties.Settings.Default.ShuffleState ? ShuffleEnum.On : ShuffleEnum.Off;
            SetShuffleIcon(_shuffleState);
            

            VolumeValue = Properties.Settings.Default.Volume;
            ProgressValue = 0.0;

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            musicEngine.SongStarted += MusicEngine_SongStarted;
            musicEngine.SongFinished += MusicEngine_SongFinished;
            musicEngine.PlaylistFinished += MusicEngine_PlaylistFinished;

            var appdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfPlayer");
            var jsonFilename = Path.Combine(appdataDir, "Folders.json");

            if (Directory.Exists(appdataDir) == false)
            {
                Directory.CreateDirectory(appdataDir);
            }

            if (File.Exists(jsonFilename))
            {
                string json = File.ReadAllText(jsonFilename);
                LoadedFolder = JsonConvert.DeserializeObject<Folder>(json);
                Properties.Settings.Default.LoadedDirectory = Path.Combine(LoadedFolder.ParentDirectory, LoadedFolder.FolderName);
            }
        }

        private void MusicEngine_PlaylistFinished(object sender, EventArgs e)
        {
            if (dispatcherTimer.IsEnabled == true)
                dispatcherTimer.Stop();
            SelectedPlaylistPosition = 0;
            _playlistPosition = 0;
            ProgressControlValue = 0.0;
            ProgressValue = 0.0;
            SetPlayPauseIcon(PlayPauseEnum.Play);
        }

        private void MusicEngine_SongFinished(object sender, EventArgs e)
        {
            if (_playlistPosition == _playlist.Count - 2)
            {
                _playlistPosition++;
                musicEngine.Play(_playlist[_playOrder[_playlistPosition]], null);
            }
            else
            {
                _playlistPosition++;
                musicEngine.Play(_playlist[_playOrder[_playlistPosition]], _playlist[_playOrder[_playlistPosition + 1]]);
            }
        }

        private void MusicEngine_SongStarted(object sender, SongStartedEventArgs e)
        {
            ArtistString = e.Artist;
            AlbumString = e.Album;
            SongTitleString = e.Title;

            if (File.Exists(e.Path + @"\folder.jpg"))
                DisplayedImagePath = new Uri(e.Path + @"\folder.jpg");
            else if (File.Exists(e.Path + @"\folder.jpeg"))
                DisplayedImagePath = new Uri(e.Path + @"\folder.jpeg");
            else if (File.Exists(e.Path + @"\cover.jpg"))
                DisplayedImagePath = new Uri(e.Path + @"\cover.jpg");
            else
                DisplayedImagePath = null;

            SelectedPlaylistPosition = _playOrder[_playlistPosition];

            SongStartedVMEventArgs args = new SongStartedVMEventArgs
            {
                Artist = e.Artist,
                Title = e.Title
            };

            OnSongStarted(args);
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
        public ICommand PlayTrackButton { get; private set; }
        public ICommand ShuffleButton { get; private set; }
        public ICommand OptionsButton { get; private set; }
        private bool EnableLoadDirButton { get; set; } = true;
        private bool EnablePlayTrackButton { get; set; } = true;
        private bool EnableShuffleButton { get; set; } = true;
        private bool EnableOptionsButton { get; set; } = true;
        public ICommand XButton { get; private set; }

        private void Closing()
        {
            musicEngine.Stop();
            
            Properties.Settings.Default.Volume = VolumeValue;
            Properties.Settings.Default.ShuffleState = _shuffleState == ShuffleEnum.On;
            Properties.Settings.Default.Save();
        }

        private void OpenOptions()
        {
            var OptionsVM = new OptionsViewModel();
            if (LoadedFolder != null)
                OptionsVM.LoadedDirectory = Properties.Settings.Default.LoadedDirectory;
            var OptionsView = new OptionsView()
            {
                DataContext = OptionsVM
            };
            OptionsView.Show();
        }

        private void ToggleShuffle()
        {
            if (_shuffleState == ShuffleEnum.On)
            {
                _shuffleState = ShuffleEnum.Off;
                if (_playlist.Count > 0)
                {
                    _playlistPosition = _playOrder[_playlistPosition];
                    _playOrder = Enumerable.Range(0, _playlist.Count).ToList();
                }
            }
            else
            {
                _shuffleState = ShuffleEnum.On;
                if (_playlist.Count > 0)
                {
                    _playOrder = shuffleList(_playOrder[_playlistPosition], Enumerable.Range(0, _playlist.Count).ToList());
                    _playlistPosition = 0;
                }
            }
            if (_playlist.Count > 0 && _playlistPosition + 1 < _playOrder.Count)
                musicEngine.LoadNextTrack(_playlist[_playOrder[_playlistPosition + 1]]);
            SetShuffleIcon(_shuffleState);
        }

        private void PlayPause()
        {
            //track is paused
            if (musicEngine.IsPaused == true)
            {
                musicEngine.Resume();
                if (dispatcherTimer.IsEnabled == false)
                    dispatcherTimer.Start();
                SetPlayPauseIcon(PlayPauseEnum.Pause);
                OnSongResumed(new EventArgs());
            }
            //no track is loaded
            else if (musicEngine.IsPlaying == false)
            {
                if (_playlistPosition == _playlist.Count - 1)
                    musicEngine.Play(_playlist[_playOrder[_playlistPosition]], null);
                else
                    musicEngine.Play(_playlist[_playOrder[_playlistPosition]], _playlist[_playOrder[_playlistPosition + 1]]);
                if (dispatcherTimer.IsEnabled == false)
                    dispatcherTimer.Start();
                SetPlayPauseIcon(PlayPauseEnum.Pause);
            }
            //track is playing
            else
            {
                musicEngine.Pause();
                if (dispatcherTimer.IsEnabled == true)
                    dispatcherTimer.Stop();
                SetPlayPauseIcon(PlayPauseEnum.Play);
                OnSongPaused(new EventArgs());
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
            _loadedDir = Path.Combine(((Folder)SelectedTreeViewItem).ParentDirectory, ((Folder)SelectedTreeViewItem).FolderName);

            if (musicEngine.IsPlaying || musicEngine.IsPaused)
            {
                _playlist.Clear();
                _playlistPosition = 0;
                musicEngine.Stop();
            }
            
            _playlist = ((Folder)SelectedTreeViewItem).GetFiles(SearchOption.AllDirectories);
            if (_shuffleState == ShuffleEnum.On)
                _playOrder = shuffleList(Enumerable.Range(0, _playlist.Count).ToList());
            else
                _playOrder = Enumerable.Range(0, _playlist.Count).ToList();

            OnPropertyChanged(nameof(Playlist));

            PlayPause();
        }

        private List<int> shuffleList(List<int> listIn)
        {
            List<int> listOut = new List<int>();

            Random rnd = new Random();

            while(listIn.Count > 0)
            {
                int index = rnd.Next(listIn.Count);
                listOut.Add(listIn[index]);
                listIn.RemoveAt(index);
            }

            return listOut;
        }

        private List<int> shuffleList(int firstElement, List<int> listIn)
        {
            List<int> listOut = new List<int>();

            int firstIndex = listIn.FindIndex(t => t == firstElement);
            listOut.Add(listIn[firstIndex]);
            listIn.RemoveAt(firstIndex);

            Random rnd = new Random();

            while (listIn.Count > 0)
            {
                int index = rnd.Next(listIn.Count);
                listOut.Add(listIn[index]);
                listIn.RemoveAt(index);
            }

            return listOut;
        }
        
        private void PlayTrack()
        {
            if (_shuffleState == ShuffleEnum.On)
            {
                if (musicEngine.IsPlaying || musicEngine.IsPaused)
                {
                    //_playlist.Clear();
                    //_playlistPosition = 0;
                    musicEngine.Stop();
                }

                _playlistPosition = 0;
                _playOrder = shuffleList(SelectedPlaylistPosition, Enumerable.Range(0, _playlist.Count).ToList());

                PlayPause();
            }
            else
            {
                if (SelectedPlaylistPosition == _playOrder[_playlistPosition])
                {
                    return;
                }
                else if (SelectedPlaylistPosition == _playOrder[_playlistPosition + 1])
                {
                    if (musicEngine.IsPaused)
                    {
                        if (dispatcherTimer.IsEnabled == false)
                            dispatcherTimer.Start();
                        SetPlayPauseIcon(PlayPauseEnum.Pause);
                    }
                    FF();
                }
                else
                {
                    if (musicEngine.IsPlaying || musicEngine.IsPaused)
                    {
                        //_playlist.Clear();
                        //_playlistPosition = 0;
                        musicEngine.Stop();
                    }
                    
                    _playlistPosition = SelectedPlaylistPosition;

                    PlayPause();
                }
            }
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

        

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSongStarted(SongStartedVMEventArgs e)
        {
            SongStarted?.Invoke(this, e);
        }

        protected virtual void OnSongPaused(EventArgs e)
        {
            SongPaused?.Invoke(this, e);
        }

        protected virtual void OnSongResumed(EventArgs e)
        {
            SongResumed?.Invoke(this, e);
        }

        protected virtual void OnThumbnailChanged(ThumbnailChangedEventArgs e)
        {
            ThumbnailChanged?.Invoke(this, e);
        }

        public event EventHandler<SongStartedVMEventArgs> SongStarted;
        public event EventHandler<EventArgs> SongPaused;
        public event EventHandler<EventArgs> SongResumed;
        public event EventHandler<ThumbnailChangedEventArgs> ThumbnailChanged;
    }

    public class ThumbnailChangedEventArgs : EventArgs
    {
        public string Path { get; set; }
    }

    public class SongStartedVMEventArgs : EventArgs
    {
        public string Artist { get; set; }
        public string Title { get; set; }
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
