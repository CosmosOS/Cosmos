using System.Windows;
using Wpf.Ui.Controls;

namespace Cosmos.Build.Builder.Views
{
    public partial class VisualStudioInstanceDialog : Window
    {
        public VisualStudioInstanceDialog()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.Watcher.Watch(this, Wpf.Ui.Appearance.BackgroundType.Mica, true);
            };
        }
    }
}
