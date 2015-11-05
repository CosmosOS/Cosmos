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
            testEngineHandler = new MainWindowHandler(message_display_list);
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
    }
}
