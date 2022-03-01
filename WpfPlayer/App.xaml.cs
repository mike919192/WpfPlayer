using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
            _ = window.ShowDialog();
        }
    }
}
