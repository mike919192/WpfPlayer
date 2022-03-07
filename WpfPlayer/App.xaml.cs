using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using WpfPlayer.ViewModel;

namespace WpfPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public MainWindowViewModel MWVM;

        SystemMediaTransportControls smtci;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MWVM = new MainWindowViewModel();

            MainWindowView window = new MainWindowView()
            {
                DataContext = MWVM
            };

            window.Show();

            var wih = new WindowInteropHelper(window);
            smtci = SystemMediaTransportControlsInterop.GetForWindow(wih.Handle);

            smtci.IsPauseEnabled = true;
            smtci.IsPlayEnabled = true;
            smtci.IsNextEnabled = true;
            smtci.IsPreviousEnabled = true;

            smtci.ButtonPressed += SystemControls_ButtonPressed;

            smtci.DisplayUpdater.Type = MediaPlaybackType.Music;

            MWVM.SongStarted += MWVM_SongStarted;
            MWVM.ThumbnailChanged += MWVM_ThumbnailChanged;
            MWVM.SongPaused += MWVM_SongPaused;
            MWVM.SongResumed += MWVM_SongResumed;
        }

        private void MWVM_SongResumed(object sender, EventArgs e)
        {
            smtci.PlaybackStatus = MediaPlaybackStatus.Playing;
        }

        private void MWVM_SongPaused(object sender, EventArgs e)
        {
            smtci.PlaybackStatus = MediaPlaybackStatus.Paused;
        }

        private void MWVM_ThumbnailChanged(object sender, ThumbnailChangedEventArgs e)
        {
            //this seems inefficient because the image is loaded again for the media control
            if (e.Path == null)
            {
                smtci.DisplayUpdater.Thumbnail = null;
            }
            else
            {
                var test = StorageFile.GetFileFromPathAsync(e.Path);
                smtci.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromFile(test.GetAwaiter().GetResult());
            }

            //smtci.DisplayUpdater.Update();
        }

        private void MWVM_SongStarted(object sender, SongStartedVMEventArgs e)
        {
            smtci.PlaybackStatus = MediaPlaybackStatus.Playing;

            //smtci.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtci.DisplayUpdater.MusicProperties.Artist = e.Artist;
            smtci.DisplayUpdater.MusicProperties.Title = e.Title;

            smtci.DisplayUpdater.Update();
        }

        void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    MWVM.PlayButton.Execute(null);
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    MWVM.PlayButton.Execute(null);
                    smtci.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;

                case SystemMediaTransportControlsButton.Next:
                    MWVM.FFButton.Execute(null);
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    MWVM.RewButton.Execute(null);
                    break;

                default:
                    break;
            }
        }
    }
}
