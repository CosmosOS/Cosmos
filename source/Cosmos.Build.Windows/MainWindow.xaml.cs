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
using System.Windows.Shapes;
using Indy.IL2CPU;

namespace Cosmos.Compiler.Builder {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Loaded += delegate(object sender, RoutedEventArgs e) {
                this.Activate();
            };
        }
        
        public void LoadControl(UserControl aControl) {
            aControl.Width = double.NaN;
            aControl.Height = double.NaN;
            Content = aControl;
        }

    }
}
