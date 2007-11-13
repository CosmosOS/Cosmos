using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace Cosmos.Kernel.LogViewer {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App: Application {
		private void Application_Startup(object sender, StartupEventArgs e) {
			System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.All;
			System.Diagnostics.PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
		}
	}
}
