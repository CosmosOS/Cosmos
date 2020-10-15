using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Cosmos.Build.Builder.Collections;
using Cosmos.Build.Builder.Models;
using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase
    {
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

        public WindowState WindowState
        {
            get => _windowState;
            set => SetAndRaiseIfChanged(ref _windowState, value);
        }

        private readonly ILogger _logger;

        private readonly IDialogService<DependencyInstallationDialogViewModel> _dependencyInstallationDialogService;

        private readonly IBuildDefinition _buildDefinition;
        private readonly Task _buildTask;

        private bool _closeWhenCompleted;

        private WindowState _windowState;

        public MainWindowViewModel(
            IDialogService<DependencyInstallationDialogViewModel> dependencyInstallationDialogService,
            IBuildDefinition buildDefinition)
        {
            _dependencyInstallationDialogService = dependencyInstallationDialogService;

            _buildDefinition = buildDefinition;

            TailItems = new ObservableFixedSizeStack<string>(TailItemCount);
            Sections = new ObservableCollection<Section>();

            Sections.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(CurrentSection));

            CopyCommand = new RelayCommand(CopyLogToClipboard);

            CloseWhenCompleted = true;
            
            _logger = new MainWindowLogger(this);

            _buildTask = BuildAsync();
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

        private async Task BuildAsync()
        {
            try
            {
                _logger.NewSection("Checking Dependencies");

                foreach (var dependency in _buildDefinition.GetDependencies())
                {
                    if (await dependency.IsInstalledAsync(CancellationToken.None).ConfigureAwait(false))
                    {
                        _logger.LogMessage($"{dependency.Name} is installed.");
                    }
                    else
                    {
                        _logger.LogMessage($"{dependency.Name} not found.");

                        using (var viewModel = new DependencyInstallationDialogViewModel(dependency))
                        {
                            _dependencyInstallationDialogService.ShowDialog(viewModel);

                            if (!viewModel.InstallationSucceeded)
                            {
                                throw new Exception($"Dependency installation failed! Dependency name: {dependency.Name}");
                            }
                        }
                    }
                }

                foreach (var buildTask in _buildDefinition.GetBuildTasks())
                {
                    _logger.NewSection(buildTask.Name);

                    await buildTask.RunAsync(_logger).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.SetError();

                _logger.NewSection("Error");
                _logger.LogMessage(e.ToString());
                _logger.SetError();
            }

            await Task.Delay(5000).ConfigureAwait(false);

            if (CloseWhenCompleted)
            {
                Application.Current.Dispatcher.Invoke(() => Application.Current?.MainWindow?.Close());
            }
            else
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
            }
        }
    }
}
