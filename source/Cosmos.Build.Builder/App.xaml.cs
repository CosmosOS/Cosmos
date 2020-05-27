using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Setup.Configuration;

using Cosmos.Build.Builder.Services;
using Cosmos.Build.Builder.ViewModels;
using Cosmos.Build.Builder.Views;

namespace Cosmos.Build.Builder
{
    public partial class App : Application
    {
        internal static IBuilderConfiguration BuilderConfiguration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (Process.GetProcessesByName("Cosmos.Build.Builder").Skip(1).Any())
            {
                ShowErrorMessageBox("Cannot run more than 1 instance of builder at the same time!");
            }

            var configuration = new CommandLineBuilderConfiguration(e.Args);

            BuilderConfiguration = configuration;

            MainWindow = new MainWindow();

            var visualStudioService = new VisualStudioService();

            var visualStudioInstances = visualStudioService.GetInstances();

            if (visualStudioInstances.Count == 0)
            {
                ShowErrorMessageBox("No Visual Studio instances found!");
                return;
            }

            ISetupInstance2 visualStudioInstance = null;

            if (configuration.VsPath != null)
            {
                visualStudioInstance = visualStudioInstances.FirstOrDefault(
                    i => String.Equals(
                        Path.GetFullPath(configuration.VsPath),
                        Path.GetFullPath(i.GetInstallationPath()),
                        StringComparison.Ordinal));
            }

            if (visualStudioInstance == null)
            {
                if (visualStudioInstances.Count == 1)
                {
                    visualStudioInstance = visualStudioInstances[0];
                }
                else
                {
                    var visualStudioInstanceDialogViewModel = new VisualStudioInstanceDialogViewModel(visualStudioService);
                    var visualStudioInstanceDialog = new VisualStudioInstanceDialog();

                    visualStudioInstanceDialog.DataContext = visualStudioInstanceDialogViewModel;

                    var result = visualStudioInstanceDialog.ShowDialog();

                    if (!(result ?? false))
                    {
                        Shutdown();
                        return;
                    }

                    visualStudioInstance = visualStudioInstanceDialogViewModel.SelectedVisualStudioInstance.SetupInstance;
                }
            }

            var innoSetupService = new InnoSetupService();
            var msBuildService = new FullMSBuildService(visualStudioInstance);

            var dependencyInstallationDialogService =
                new DialogService<DependencyInstallationDialog, DependencyInstallationDialogViewModel>(
                    () => new DependencyInstallationDialog(), MainWindow);
            
            var buildDefinition = new CosmosBuildDefinition(innoSetupService, msBuildService, visualStudioInstance);

            // show first, or setting owner on dialog windows may fail, as the main window may have not been shown yet.
            MainWindow.Show();
            MainWindow.DataContext = new MainWindowViewModel(dependencyInstallationDialogService, buildDefinition);

            base.OnStartup(e);
        }

        private static void ShowErrorMessageBox(string message) =>
            MessageBox.Show(message, "Cosmos Kit Builder", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
