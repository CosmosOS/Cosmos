using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;

namespace Cosmos.Kernel.LogViewer.Overview {
	public class LoggedHexNumberToUInt32Converter: IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			string xValue = value.ToString();
			XmlAttribute xAttrib = value as XmlAttribute;
			if (xAttrib != null) {
				xValue = xAttrib.Value;
			}
			xValue = xValue.Substring(2);
			string xType = "**NULL**";
			if (parameter != null) {
				xType = parameter.GetType().FullName;
			}
			Console.WriteLine("Param: {0} ({1})", parameter, xType);
			return UInt32.Parse(xValue, NumberStyles.HexNumber) / UInt32.Parse((parameter??"").ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return null;
		}
	}
}