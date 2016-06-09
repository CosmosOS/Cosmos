using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFMachine.Options
{
    /// <summary>
    /// Interaction logic for GameDirectory.xaml
    /// </summary>
    public partial class GameDirectory : UserControl
    {
        public event RoutedEventHandler Click;

        public GameDirectory(String text)
        {
            InitializeComponent();
            lDir.Content = text;
        }

        private void bRemove_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null) Click(this, new RoutedEventArgs());
        }

        public String Directory
        {
            get { return lDir.Content.ToString(); }
        }
    }
}
