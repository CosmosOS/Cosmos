using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Cosmos.TestRunner.UI.ViewModels;

namespace Cosmos.TestRunner.UI.Views
{
    internal class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
            this.AttachDevTools();

            DataContext = new SettingsDialogViewModel(this);
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
