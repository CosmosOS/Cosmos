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

namespace Cosmos.Build.Windows.Config {
	/// <summary>
	/// Interaction logic for ChooseVSWindow.xaml
	/// </summary>
	public partial class ChooseVSWindow: Window {
		public ChooseVSWindow() {
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			this.DialogResult = true;
			Close();
		}
	}
}
