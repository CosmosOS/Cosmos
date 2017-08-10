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

namespace WPFMachine.Options {
    /// <summary>
    /// Interaction logic for FontDropDown.xaml
    /// </summary>
    public partial class FontDropDown : UserControl {
        public FontDropDown() {
            InitializeComponent();
        }

        public System.Collections.IEnumerable ItemsSource {
            get { return fontComboFast.ItemsSource; }
            set { fontComboFast.ItemsSource = value; }
        }

        public Object SelectedItem {
            get { return fontComboFast.SelectedItem; }
            set { fontComboFast.SelectedItem = value; }
        }
    }
}
