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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cosmos.TestRunner.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowHandler testEngineHandler = null;
        public MainWindow()
        {
            InitializeComponent();

            message_display_list.Focus();

            testEngineHandler = new MainWindowHandler(message_display_list);

            testEngineHandler.TestFinished += delegate
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    save_log_btn.IsEnabled = true;
                }));
            };

            testEngineHandler.RunTestEngine();
        }

        private void h_messages_btn_Click(object sender, RoutedEventArgs e)
        {
            message_display_list.Visibility = Visibility.Collapsed;
        }

        private void s_messages_btn_Click(object sender, RoutedEventArgs e)
        {
            message_display_list.Visibility = Visibility.Visible;
        }

        private void save_log_btn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();

            saveDialog.DefaultExt = ".xml";
            saveDialog.FileName = "TestResult.xml";
            saveDialog.Filter = "XML Documents (.xml)|*.xml";

            if(saveDialog.ShowDialog() ?? false)
            {
                string filename = saveDialog.FileName;

                testEngineHandler.outputHandler.SaveToFile(filename);
            }
        }

        private void copy_message_menu_item_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((ListViewLogMessage)message_display_list.SelectedItem).Message);
        }
    }
}
