using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Cosmos.Build.Builder.Views
{
    public partial class VisualStudioInstanceDialog : Window
    {
        public VisualStudioInstanceDialog()
        {
            InitializeComponent();
            SystemThemeWatcher.Watch(this);
        }
    }
}
