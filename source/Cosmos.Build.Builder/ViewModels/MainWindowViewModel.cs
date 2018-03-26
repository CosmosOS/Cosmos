using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using Cosmos.Build.Builder.Collections;
using Cosmos.Build.Builder.Models;

namespace Cosmos.Build.Builder.ViewModels
{
    internal class MainWindowViewModel
    {
        private const int TailItemCount = 10;
        
        public ICommand CopyCommand { get; }

        public ObservableFixedSizeStack<string> TailItems { get; }
        public ObservableCollection<Section> Sections { get; }

        public Section CurrentSection => Sections.LastOrDefault();

        public StringBuilder LogBuilder { get; set; }

        public MainWindowViewModel()
        {
            CopyCommand = new Command(this);

            TailItems = new ObservableFixedSizeStack<string>(TailItemCount);
            Sections = new ObservableCollection<Section>();
        }

        private class Command : ICommand
        {
            public event EventHandler CanExecuteChanged;

            private MainWindowViewModel _viewModel;
            private Func<bool> _canExecute;

            public Command(MainWindowViewModel viewModel, Func<bool> canExecute = null)
            {
                _viewModel = viewModel;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
            public void Execute(object parameter) => Clipboard.SetText(_viewModel.LogBuilder.ToString());

            public void RaiseCanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged?.Invoke(sender, e);
        }
    }
}
