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

namespace Cosmos.Build.Windows {
    /// <summary>
    /// Interaction logic for BuildOptionsWindow.xaml
    /// </summary>
    public partial class BuildOptionsWindow : Window {
        public BuildOptionsWindow() {
            InitializeComponent();

            KeyDown += new KeyEventHandler(BuildOptionsWindow_KeyDown);
            
            textBuildPath.Text = Builder.GetBuildPath();
            textBuildPath.IsEnabled = false;

            foreach (var xTarget in Enum.GetNames(typeof(Builder.Target))) {
                lboxTargets.Items.Add((lboxTargets.Items.Count + 1).ToString() + ": " + xTarget);
            }
        }

        void BuildOptionsWindow_KeyDown(object sender, KeyEventArgs e) {
            char xChar = e.Key.ToString()[0];
            if (Char.IsDigit(xChar)) {
                int xValue = int.Parse(xChar.ToString());
                if (xValue > 0) {
                    if (xValue <= lboxTargets.Items.Count) {
                        string xType = (string)(lboxTargets.Items[xValue - 1]);
                        var xBuilder = new Builder();
                        xBuilder.Build((Builder.Target)Enum.Parse(typeof(Builder.Target), xType));
                    }
                }
                e.Handled = true;
            }
        }
    }
}
