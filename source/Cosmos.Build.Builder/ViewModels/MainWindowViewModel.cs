using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Cosmos.Build.Builder.Collections;
using Cosmos.Build.Builder.Models;

namespace Cosmos.Build.Builder.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private const int ReleaseNumber = 106027;

        private const int TailItemCount = 10;

        public ObservableFixedSizeStack<string> TailItems { get; }
        public ObservableCollection<Section> Sections { get; }

        public Section CurrentSection => Sections.LastOrDefault();

        public ICommand CopyCommand { get; }

        public bool CloseWhenCompleted
        {
            get => _closeWhenCompleted;
            set => SetAndRaiseIfChanged(ref _closeWhenCompleted, value);
        }

        private CosmosTask _cosmosTask;
        private Task _cosmosTaskTask;

        private bool _closeWhenCompleted;

        public MainWindowViewModel()
        {
            TailItems = new ObservableFixedSizeStack<string>(TailItemCount);
            Sections = new ObservableCollection<Section>();

            CopyCommand = new RelayCommand(CopyLogToClipboard);

            var logger = new MainWindowLogger(this);

            _closeWhenCompleted = true;

            var cosmosDir = Directory.GetCurrentDirectory();

            _cosmosTask = new CosmosTask(logger, cosmosDir, ReleaseNumber);
            _cosmosTaskTask = Task.Run((Action)_cosmosTask.Run);
            _cosmosTaskTask.ContinueWith(CosmosTaskFinishedAsync);
        }

        private void CopyLogToClipboard(object parameter) => Clipboard.SetText(BuildLog());

        private string BuildLog()
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

        private Task CosmosTaskFinishedAsync(Task task) =>
            Application.Current.Dispatcher.InvokeAsync(
                async () =>
                {
                    var mainWindow = Application.Current.MainWindow;

                    await Task.Delay(5000);

                    if (CloseWhenCompleted)
                    {
                        mainWindow.Close();
                    }
                    else
                    {
                        if (mainWindow.WindowState == WindowState.Maximized)
                        {
                            mainWindow.WindowState = WindowState.Normal;
                        }
                    }
                }).Task;
    }
}
