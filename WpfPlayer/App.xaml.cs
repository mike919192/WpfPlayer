using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.Media;
using WpfPlayer.ViewModel;

namespace WpfPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public MainWindowViewModel MWVM;
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
            var smtci = SystemMediaTransportControlsInterop.GetForWindow(wih.Handle);

            smtci.IsPauseEnabled = true;
            smtci.IsPlayEnabled = true;
            smtci.IsNextEnabled = true;
            smtci.IsPreviousEnabled = true;

            smtci.ButtonPressed += SystemControls_ButtonPressed;

            smtci.PlaybackStatus = MediaPlaybackStatus.Playing;
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
