using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.UI.ViewModels;

namespace Cosmos.TestRunner.UI.Views
{
    internal class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.AttachDevTools();

            Dispatcher.UIThread.InvokeAsync(async () => await ShowSettingsDialog().ConfigureAwait(false));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task ShowSettingsDialog()
        {
            var xSettingsDialog = new SettingsDialog();
            var xEngineConfiguration = await xSettingsDialog.ShowDialog<IEngineConfiguration>();

            if (xEngineConfiguration == null)
            {
                Application.Current.Exit();
                return;
            }

            DataContext = new MainWindowViewModel(xEngineConfiguration);
        }
    }
}
