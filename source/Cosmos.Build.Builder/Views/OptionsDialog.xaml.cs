using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cosmos.Build.Builder.BuildTasks;
using Wpf.Ui.Appearance;

namespace Cosmos.Build.Builder.Views
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        internal BuilderConfiguration BuildOptions { get; private set; }
        public OptionsDialog()
        {
            InitializeComponent();
            SystemThemeWatcher.Watch(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            BuildOptions = new()
            {
                BuildExtensions = chkBuildExtensions.IsChecked.Value,
                UserKit = chkBuildUserkit.IsChecked.Value
            };
            DialogResult = true;
        }
    }
}
