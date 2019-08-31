using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Cosmos.Build.Builder.ViewModels
{
    internal sealed class DependencyInstallationDialogViewModel : ViewModelBase, IDisposable
    {
        private const string InstallationSucceededText = "{0} installed successfully!";
        private const string InstallationFailedText = "{0} failed to install!";

        public IDependency Dependency { get; }

        public bool IsNotInstallingYet
        {
            get => _isNotInstallingYet;
            private set => SetAndRaiseIfChanged(ref _isNotInstallingYet, value);
        }

        public bool IsInstalling
        {
            get => _isInstalling;
            private set => SetAndRaiseIfChanged(ref _isInstalling, value);
        }

        public bool IsInstallationCompleted
        {
            get => _isInstallationCompleted;
            private set => SetAndRaiseIfChanged(ref _isInstallationCompleted, value);
        }

        public string InstallationCompletedText
        {
            get => _installationCompletedText;
            private set => SetAndRaiseIfChanged(ref _installationCompletedText, value);
        }

        public bool InstallationSucceeded { get; private set; }

        public ICommand InstallCommand { get; }
        public ICommand CancelCommand { get; }

        public ICommand CancelInstallationCommand { get; }

        public ICommand OkCommand { get; }

        private bool _isNotInstallingYet;
        private bool _isInstalling;
        private bool _isInstallationCompleted;
        private string _installationCompletedText;

        private Task _installTask;
        private CancellationTokenSource _installTaskCancellationTokenSource;

        public DependencyInstallationDialogViewModel(IDependency dependency)
        {
            Dependency = dependency;

            IsNotInstallingYet = true;

            InstallCommand = new RelayCommand(Install);
            CancelCommand = new RelayCommand(p => Close(p as Window, false));

            CancelInstallationCommand = new RelayCommand(CancelInstallation);

            OkCommand = new RelayCommand(p => Close(p as Window, true));
        }

        public void Dispose()
        {
            _installTask.Dispose();
            _installTaskCancellationTokenSource.Dispose();
        }

        private void Install(object parameter) => _installTask = InstallAsync();
        private void CancelInstallation(object parameter) => _installTaskCancellationTokenSource.Cancel();

        private static void Close(Window window, bool? dialogResult)
        {
#if DEBUG
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }
#endif

            window.DialogResult = true;
            window.Close();
        }

        private async Task InstallAsync()
        {
            _installTaskCancellationTokenSource = new CancellationTokenSource();

            IsNotInstallingYet = false;
            IsInstalling = true;

            await Dependency.InstallAsync(_installTaskCancellationTokenSource.Token).ConfigureAwait(false);
            await InstallationFinishedAsync().ConfigureAwait(false);
        }

        private async Task InstallationFinishedAsync()
        {
            IsInstalling = false;
            IsInstallationCompleted = true;

            InstallationSucceeded = await Dependency.IsInstalledAsync(
                _installTaskCancellationTokenSource.Token).ConfigureAwait(false);
            InstallationCompletedText = String.Format(
                InstallationSucceeded ? InstallationSucceededText : InstallationFailedText, Dependency.Name);
        }
    }
}
