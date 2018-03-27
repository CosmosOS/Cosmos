using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Cosmos.Build.Builder.Collections;
using Cosmos.Build.Builder.Models;

namespace Cosmos.Build.Builder.ViewModels
{
    internal class MainWindowViewModel
    {
        private const int TailItemCount = 10;

        public ObservableFixedSizeStack<string> TailItems { get; }
        public ObservableCollection<Section> Sections { get; }

        public Section CurrentSection => Sections.LastOrDefault();

        public ICommand CopyCommand { get; }

        private ILogger _logger;

        public MainWindowViewModel()
        {
            TailItems = new ObservableFixedSizeStack<string>(TailItemCount);
            Sections = new ObservableCollection<Section>();

            CopyCommand = new CopyLogCommand(this);

            _logger = new MainWindowLogger(this);
        }

        public string BuildLog()
        {
            var log = @"
========================================
    Builder Log
========================================

";

            foreach (var section in Sections)
            {
                log += $@"
========================================
    {section.Name}
========================================

";
                log += section.Log + Environment.NewLine;
            }

            return log;
        }

        private class CopyLogCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            private MainWindowViewModel _viewModel;
            private Func<bool> _canExecute;

            public CopyLogCommand(MainWindowViewModel viewModel, Func<bool> canExecute = null)
            {
                _viewModel = viewModel;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
            public void Execute(object parameter) => Clipboard.SetText(_viewModel.BuildLog());

            public void RaiseCanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged?.Invoke(sender, e);
        }
    }
}
