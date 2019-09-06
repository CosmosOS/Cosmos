using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Cosmos.Build.Builder.Collections;
using Cosmos.Build.Builder.Models;
using Cosmos.Build.Builder.Services;
using Microsoft.Win32;

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

        internal static string GetVisualStudioInstalledPath()
        {
            var visualStudioInstalledPath = string.Empty;
            var visualStudioRegistryPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7");
            if (visualStudioRegistryPath != null)
            {
                //Check for the latest version if there is one
                string[] subkeynames = visualStudioRegistryPath.GetSubKeyNames();
                double versionnumb = 0.0;
                foreach (string m in subkeynames)
                {
                    double versionnumber = Double.Parse(m);
                    if(versionnumber > versionnumb) { versionnumb = versionnumber; }
                }
                visualStudioInstalledPath = visualStudioRegistryPath.GetValue(versionnumb.ToString(), string.Empty) as string;
            }

            return visualStudioInstalledPath;
        }

        private async Task BuildAsync()
        {
            try
            {
                _logger.NewSection("Checking right paths");
                string vspath = GetVisualStudioInstalledPath();
                //This can help remedy problems with newer versions
                if(vspath == string.Empty && System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") != "x64") { _logger.LogMessage("Using Program Files (x64) as a reg key couldnt be found."); vspath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles); } else if
                    (vspath == string.Empty && System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") != "x86") { _logger.LogMessage("Using Program Files (x86) as a reg key couldnt be found."); vspath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86); } else
                { _logger.LogMessage("Visual Studio regkey path found: " + vspath); }

                //Checking if vspath drive letter matches the executable path and if it doesnt throw an exception
                if(Path.GetPathRoot(vspath) != Path.GetPathRoot(System.Reflection.Assembly.GetExecutingAssembly().Location))
                {
                    throw new Exception("Dependency installation failed as the install location must be on the same drive as the install of VS");
                }
                else
                {
                    _logger.LogMessage("Cosmos is on the correct drive!");
                }

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
