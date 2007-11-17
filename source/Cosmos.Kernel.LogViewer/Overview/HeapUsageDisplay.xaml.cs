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
using System.Xml;

namespace Cosmos.Kernel.LogViewer.Overview {
	/// <summary>
	/// Interaction logic for HeapUsageDisplay.xaml
	/// </summary>
	public partial class HeapUsageDisplay: UserControl {
		public HeapUsageDisplay() {
			InitializeComponent();
		}

		public int TotalHeapUsage {
			get {
				XmlDataProvider xXML = (XmlDataProvider)FindResource("LogFile");
				int xValue = 0;
				foreach(XmlAttribute xa in xXML.Document.SelectNodes("//MM_Alloc/@Length")) {
					xValue += Int32.Parse(xa.Value.Substring(2), System.Globalization.NumberStyles.HexNumber);
				}
				return xValue;
			}
		}
	}
}
