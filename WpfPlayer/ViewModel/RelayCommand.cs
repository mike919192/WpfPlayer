using System;
using System.Windows.Input;

//Creates RelayCommand Class
namespace WpfPlayer.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        //RelayCommandFunction
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute;
        }

        //Sees if RelayCommand can action
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        //Execute's RelayCommand
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}