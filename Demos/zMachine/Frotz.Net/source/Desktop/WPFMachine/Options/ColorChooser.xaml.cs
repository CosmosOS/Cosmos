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
    /// Interaction logic for ColorChooser.xaml
    /// </summary>
    public partial class ColorChooser : UserControl {
        public ColorChooser() {
            InitializeComponent();
        }

        private Color _selectedColor;
        public Color SelectedColor {
            get {
                return _selectedColor;
            }
            set {
                tbColor.Background = new SolidColorBrush(value);
                _selectedColor = value;
            }
        }

        private void tbColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var cpd = new Microsoft.Samples.CustomControls.ColorPickerDialog();

            cpd.StartingColor = _selectedColor;

            if (cpd.ShowDialog() == true) {
                SelectedColor = cpd.SelectedColor;
            }
        }
    }
}
