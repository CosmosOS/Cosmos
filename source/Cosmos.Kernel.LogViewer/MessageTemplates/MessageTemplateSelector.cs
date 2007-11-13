using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Cosmos.Kernel.LogViewer.MessageTemplates {
	public class MessageTemplateSelector: DataTemplateSelector {

		public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container) {
			XmlNode xn = (XmlNode)item;
			switch (xn.Name) {
				case "Log": {
						return (DataTemplate)Application.Current.FindResource("Everything_Container");
					}
				case "Message": {
						return (DataTemplate)Application.Current.FindResource("Everything_NormalMessage");
					}
			}
			return base.SelectTemplate(item, container);
		}
	}
}