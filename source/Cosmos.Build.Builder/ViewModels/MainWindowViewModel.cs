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
using Cosmos.Build.Builder.Views;

namespace Cosmos.Build.Builder.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        private const int TailItemCount = 10;

        public ObservableFixedSizeStack<string> TailItems { get; }
        public ObservableCollection<Section> Sections { get; }

        public Section CurrentSection => Sections.LastOrDefault();

        public ICommand CopyCommand { get; set; }

        public ICommand PostPaste { get; set; }

        public ICommand RetryBuild { get; set; }

        public bool CloseWhenCompleted { get; set; }

        public bool AnErrorOccurred { get; set; }

        public MainWindow Window
        {
            get => _window;
            set => SetAndRaiseIfChanged(ref _window, value);
        }

        private readonly ILogger _logger;

        private readonly IDialogService<DependencyInstallationDialogViewModel> _dependencyInstallationDialogService;

        private readonly IBuildDefinition _buildDefinition;

        private bool _buildCancel;

        private Task _buildTask;

        private MainWindow _window;

        public MainWindowViewModel(
            IDialogService<DependencyInstallationDialogViewModel> dependencyInstallationDialogService,
            IBuildDefinition buildDefinition, MainWindow win)
        {
            _window = win;

            _dependencyInstallationDialogService = dependencyInstallationDialogService;

            _buildDefinition = buildDefinition;

            TailItems = new ObservableFixedSizeStack<string>(TailItemCount);
            Sections = new ObservableCollection<Section>();

            Sections.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(CurrentSection));

            CopyCommand = new RelayCommand(CopyLogToClipboard);

            PostPaste = new RelayCommand(PostPasteCommand);

            RetryBuild = new RelayCommand(RetryBuildCommand);

            _logger = new MainWindowLogger(this);

            _buildTask = BuildAsync();
        }

        private void RetryBuildCommand(object obj)
        {
            _buildCancel = true;
            MainWindow win = new();
            win.Show();
            _dependencyInstallationDialogService.SetAnotherOwner(win);
            Window.AppShutdown = false;
            Window.Close();
            win.DataContext = new MainWindowViewModel(_dependencyInstallationDialogService, _buildDefinition, win);
        }

        private void CopyLogToClipboard(object parameter) => Clipboard.SetText(BuildLog());

        private void PostPasteCommand(object parameter) => InternalPostPaste();

        private string BuildLog()
        {
            var log = @"
========================================
    Build Log
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

        private void InternalPostPaste()
        {
            try
            {
                string baseUrl = "https://www.toptal.com/developers/hastebin/";
                var hasteBinClient = new HasteBinClient(baseUrl);
                HasteBinResult result = hasteBinClient.Post(BuildLog()).Result;

                if (result.IsSuccess)
                {
                    Views.MessageBox.Show($"link:{baseUrl}{result.Key}");
                }
                else
                {
                    Views.MessageBox.Show($"Failed, status code was {result.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Views.MessageBox.Show(e.Message);
            }
        }

        private async Task BuildAsync()
        {
            _logger.NewSection("Checking Dependencies...");
            try
            {
                foreach (var dependency in _buildDefinition.GetDependencies())
                {
                    if (await dependency.IsInstalledAsync(CancellationToken.None).ConfigureAwait(false))
                    {
                        _logger.LogMessage($"{dependency.Name} is installed.");
                    }
                    else
                    {
                        _logger.LogMessage($"{dependency.Name} was not found. Install {dependency.OtherDependencysThatAreMissing.TrimEnd(',')}");

                        if (dependency.ShouldInstallByDefault)
                        {
                            using (var viewModel = new DependencyInstallationDialogViewModel(dependency))
                            {
                                _dependencyInstallationDialogService.ShowDialog(viewModel);

                                if (!viewModel.InstallationSucceeded)
                                {
                                    throw new Exception($"Dependency installation failed! Dependency name: {dependency.Name}");
                                }
                            }
                        }
                        else
                        {
                            Views.MessageBox.Show($"{dependency.Name} is not installed. Please {dependency.OtherDependencysThatAreMissing}");
                            _logger.SetError();
                            _logger.NewSection("Error");
                            _logger.LogMessage($"{dependency.Name} not found.");
                            _logger.SetError();
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OnError("Error while installing dependencies: " + e.Message);
                return;
            }

            try
            {
                foreach (var buildTask in _buildDefinition.GetBuildTasks())
                {
                    if (_buildCancel) { throw new TaskCanceledException(); }

                    _logger.NewSection(buildTask.Name);

                    await buildTask.RunAsync(_logger).ConfigureAwait(false);
                }

                Window.AllTasksCompleted = true;

                if (CloseWhenCompleted)
                {
                    Application.Current.Dispatcher.Invoke(() => Application.Current?.MainWindow?.Close());
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
                return;
            }

            await Task.Delay(5000).ConfigureAwait(false);

            if (CloseWhenCompleted)
            {
                Application.Current.Dispatcher.Invoke(() => Application.Current?.MainWindow?.Close());
            }
            else
            {
                if (Window.WindowState == WindowState.Maximized)
                {
                    Window.WindowState = WindowState.Normal;
                }
            }
        }
        public void OnError(string message)
        {
            _logger.SetError();

            _logger.NewSection("Error");
            _logger.LogMessage(message);
            _logger.SetError();
        }
    }
}
