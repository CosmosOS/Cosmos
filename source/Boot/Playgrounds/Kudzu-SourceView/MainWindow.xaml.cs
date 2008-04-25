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

namespace Kudzu_SourceView {
    public partial class Window1 : Window {
        public Window1() {
            InitializeComponent();
            button1.Click += new RoutedEventHandler(button1_Click);
        }

        void button1_Click(object sender, RoutedEventArgs e) {
            var xView = new Cosmos.Build.Windows.DebugWindow();
            xView.LoadSourceFile(@"C:\source\Cosmos\source\Cosmos\Cosmos.Kernel\Keyboard.cs");
            xView.SelectText(1, 4, 3, 8);
            xView.Show();
        }

    }
}
