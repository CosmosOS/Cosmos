using System;
using System.Windows.Input;

namespace Cosmos.Build.Builder.ViewModels
{
    internal class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute?.Invoke(parameter);

        public void RaiseCanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged?.Invoke(sender, e);
    }
}
