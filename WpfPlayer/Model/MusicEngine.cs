using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using NAudio.Wave;
using System;
using System.Collections.Generic;
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

        public double Position
        {
            get
            {
                return (double)audioFile.Position / (double)audioFile.Length;
            }
            set
            {
                audioFile.Position = (long)(value * audioFile.Length);
            }
        }

        public TimeSpan TimeRemaining
        {
            get
            {
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

        }

        public void Play(string currentTrackFile, string nextTrackFile)
        {
            

            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (audioFile == null)
            {
                if (currentTrackFile == nextAudioFile?.FileName)
                {
                    audioFile = nextAudioFile;
                    ArtistString = _nextArtistString;
                    AlbumString = _nextAlbumString;
                    SongTitleString = _nextSongTitleString;
                }
                else
                {
                    audioFile = new AudioFileReader(currentTrackFile);
                    ShellObject musicFile = ShellObject.FromParsingName(currentTrackFile);
                    ArtistString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Music.AlbumArtist));
                    AlbumString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Music.AlbumTitle));
                    SongTitleString = GetValue(musicFile.Properties.GetProperty(SystemProperties.System.Title));
                }

                outputDevice.Init(audioFile);

                //_nextTrackFile = nextTrackFile;
                if (nextTrackFile != null)
                {
                    LoadNextTrack(nextTrackFile);
                    //trd = new Thread(new ThreadStart(LoadNextTrackThread));
                    //trd.IsBackground = true;
                    //trd.Start();
                }
                else
                {
                    _nextTrackFile = nextTrackFile;
                }
            }
            outputDevice.Volume = (float)_volume;
            outputDevice.Play();

            SongStartedEventArgs args = new SongStartedEventArgs
            {
                Path = Path.GetDirectoryName(currentTrackFile),
                Filename = Path.GetFileName(currentTrackFile),
                Artist = ArtistString,
                Album = AlbumString,
                Title = SongTitleString
            };
            OnSongStarted(args);
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
