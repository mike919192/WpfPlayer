using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                OnPropertyChanged(nameof(LoadedDirectory));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
