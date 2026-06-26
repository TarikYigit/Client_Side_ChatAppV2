using Client_Side_ChatApp.Core;
using System;
using System.Windows.Input;
namespace Client_Side_ChatApp.Core
{


    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null) 
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged 
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter); //if button is enabled, can execute
        public void Execute(object parameter) => _execute(parameter);   //run the function when button is clicked
    }
}