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
	public partial class ViewSourceWindow: Window {
		public ViewSourceWindow() {
			InitializeComponent();
		}

		public int CharStart;
		public int CharLength;

        public void LoadSourceFile(string aPathname) {
            // Old
            tboxSource.Text = System.IO.File.ReadAllText(aPathname);
            // New
            var xSourceCode = System.IO.File.ReadAllLines(aPathname);
            var xPara = new Paragraph();
            fdsvSource.Document.Blocks.Add(xPara);
            foreach (var xLine in xSourceCode) {
                xPara.Inlines.Add(xLine);
                xPara.Inlines.Add(new LineBreak());
            }
        }

		private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Old
			tboxSource.Focus();
			tboxSource.Select(CharStart, CharLength);
            // New
		}

	}
}