using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfPlayer.Model
{
    class MusicEngine
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private AudioFileReader nextAudioFile;

        Thread trd;

        public string ArtistString;
        public string AlbumString;
        public string SongTitleString;
        private string _nextArtistString;
        private string _nextAlbumString;
        private string _nextSongTitleString;

        private string _nextTrackFile;

        private BackgroundWorker fileWorker = new BackgroundWorker();

        public double Position
        {
            get
            {
                if (audioFile == null)
                    return 0.0;
                else
                    return (double)audioFile.Position / (double)audioFile.Length;
            }
            set
            {
                if (audioFile != null)
                    audioFile.Position = (long)(value * audioFile.Length);
            }
        }

        public TimeSpan TimeRemaining
        {
            get
            {
                if (audioFile == null)
                    return new TimeSpan();
                else
                    return (1 - Position) * audioFile.TotalTime;
            }
        }

        private double _volume;
        public double Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
                if (outputDevice != null)
                    outputDevice.Volume = (float)value;
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (outputDevice == null)
                    return false;
                else
                    return outputDevice.PlaybackState == PlaybackState.Playing;
            }
        }

        public bool IsPaused
        {
            get
            {
                if (outputDevice == null)
                    return false;
                else
                    return outputDevice.PlaybackState == PlaybackState.Paused;
            }
        }

        public MusicEngine()
        {
            fileWorker.DoWork += FileWorker_DoWork;
            fileWorker.RunWorkerCompleted += FileWorker_RunWorkerCompleted;
            //fileWorker.WorkerSupportsCancellation = true;
        }

        private void FileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var workerResult = (fileWorkerResult)e.Result;
            ArtistString = workerResult.artistString;
            AlbumString = workerResult.albumString;
            SongTitleString = workerResult.songTitleString;

            audioFile = workerResult.audioFile;
            outputDevice.Init(audioFile);

            //_nextTrackFile = nextTrackFile;
            if (workerResult.workerArgs.nextTrackFile != null)
            {
                LoadNextTrack(workerResult.workerArgs.nextTrackFile);
                //trd = new Thread(new ThreadStart(LoadNextTrackThread));
                //trd.IsBackground = true;
                //trd.Start();
            }
            else
            {
                _nextTrackFile = workerResult.workerArgs.nextTrackFile;
            }
            //}
            outputDevice.Volume = (float)_volume;
            outputDevice.Play();

            SongStartedEventArgs args = new SongStartedEventArgs
            {
                Path = Path.GetDirectoryName(workerResult.workerArgs.currentTrackFile),
                Filename = Path.GetFileName(workerResult.workerArgs.currentTrackFile),
                Artist = ArtistString,
                Album = AlbumString,
                Title = SongTitleString
            };
            OnSongStarted(args);
        }

        private void FileWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var workerArgs = (fileWorkerArgs)e.Argument;
            var workerResult = new fileWorkerResult();

            if (workerArgs.currentTrackFile == nextAudioFile?.FileName)
            {
                workerResult.audioFile = nextAudioFile;
                workerResult.artistString = _nextArtistString;
                workerResult.albumString = _nextAlbumString;
                workerResult.songTitleString = _nextSongTitleString;
            }
            else
            {
                workerResult.audioFile = new AudioFileReader(workerArgs.currentTrackFile);
                var musicFile = ShellObject.FromParsingName(workerArgs.currentTrackFile);
                workerResult.artistString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Music.AlbumArtist));
                workerResult.albumString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Music.AlbumTitle));
                workerResult.songTitleString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Title));
            }
            workerResult.workerArgs = workerArgs;

            e.Result = workerResult;
        }

        struct fileWorkerArgs
        {
            public string currentTrackFile;
            public string nextTrackFile;
        }

        struct fileWorkerResult
        {
            public AudioFileReader audioFile;
            public string artistString;
            public string albumString;
            public string songTitleString;
            public fileWorkerArgs workerArgs;
        }

        public void Play(string currentTrackFile, string nextTrackFile)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }

            var workerArgs = new fileWorkerArgs
            {
                currentTrackFile = currentTrackFile,
                nextTrackFile = nextTrackFile
            };

            fileWorker.RunWorkerAsync(workerArgs);

            //if (audioFile == null)
            //{
            //    if (currentTrackFile == nextAudioFile?.FileName)
            //    {
            //        audioFile = nextAudioFile;
            //        ArtistString = _nextArtistString;
            //        AlbumString = _nextAlbumString;
            //        SongTitleString = _nextSongTitleString;
            //    }
            //    else
            //    {
            //        audioFile = new AudioFileReader(currentTrackFile);
            //        ShellObject musicFile = ShellObject.FromParsingName(currentTrackFile);
            //        ArtistString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Music.AlbumArtist));
            //        AlbumString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Music.AlbumTitle));
            //        SongTitleString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Title));
            //    }

            //    outputDevice.Init(audioFile);

            //    //_nextTrackFile = nextTrackFile;
            //    if (nextTrackFile != null)
            //    {
            //        LoadNextTrack(nextTrackFile);
            //        //trd = new Thread(new ThreadStart(LoadNextTrackThread));
            //        //trd.IsBackground = true;
            //        //trd.Start();
            //    }
            //    else
            //    {
            //        _nextTrackFile = nextTrackFile;
            //    }
            ////}
            //outputDevice.Volume = (float)_volume;
            //outputDevice.Play();

            //SongStartedEventArgs args = new SongStartedEventArgs
            //{
            //    Path = Path.GetDirectoryName(currentTrackFile),
            //    Filename = Path.GetFileName(currentTrackFile),
            //    Artist = ArtistString,
            //    Album = AlbumString,
            //    Title = SongTitleString
            //};
            //OnSongStarted(args);
        }

        public void Pause()
        {
            outputDevice.Pause();
        }

        public void Resume()
        {
            outputDevice.Play();
        }

        public void Stop()
        {
            if (outputDevice != null)
            {
                outputDevice.PlaybackStopped -= OnPlaybackStopped;
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }
            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }
        }

        private static string GetValue(IShellProperty value)
        {
            if (value == null || value.ValueAsObject == null)
            {
                return string.Empty;
            }
            return value.ValueAsObject.ToString();
        }

        public void LoadNextTrack(string nextTrackFile)
        {
            if (nextTrackFile == null)
            {
                _nextTrackFile = null;
            }
            //check if new next track is the same as what is already loaded
            else if (_nextTrackFile != nextTrackFile)
            {
                _nextTrackFile = nextTrackFile;
                trd = new Thread(new ThreadStart(LoadNextTrackThread));
                trd.IsBackground = true;
                trd.Start();
            }
        }

        private void LoadNextTrackThread()
        {
            Debugger.Log(1, "Threading", "Started loading next file\n");
            nextAudioFile = new AudioFileReader(_nextTrackFile);
            ShellObject musicFile = ShellObject.FromParsingName(_nextTrackFile);
            _nextArtistString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Music.DisplayArtist));
            _nextAlbumString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Music.AlbumTitle));
            _nextSongTitleString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Title));
            Debugger.Log(1, "Threading", "Finished loading next file\n");
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFile.Dispose();
            audioFile = null;

            if (_nextTrackFile != null)
            {
                //_playlistPosition++;
                //OnPropertyChanged(nameof(PlaylistPosition));
                //Play();
                OnSongFinished(new EventArgs());
            }
            else
            {
                //dispatcherTimer.Stop();
                OnPlaylistFinished(new EventArgs());
            }
        }

        public void Rew()
        {
            Position = 0.0;
        }

        public void FF()
        {
            outputDevice.Stop();
        }

        protected virtual void OnSongStarted(SongStartedEventArgs e)
        {
            SongStarted?.Invoke(this, e);
        }
        protected virtual void OnPlaylistFinished(EventArgs e)
        {
            PlaylistFinished?.Invoke(this, e);
        }
        protected virtual void OnSongFinished(EventArgs e)
        {
            SongFinished?.Invoke(this, e);
        }

        public event EventHandler<SongStartedEventArgs> SongStarted;
        public event EventHandler<EventArgs> PlaylistFinished;
        public event EventHandler<EventArgs> SongFinished;
    }

    public class SongStartedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public string Filename { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
    }
}
