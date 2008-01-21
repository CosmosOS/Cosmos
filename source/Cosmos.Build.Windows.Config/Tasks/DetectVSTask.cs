using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows;
using System.IO;

namespace Cosmos.Build.Windows.Config.Tasks {
	public class DetectVSTask: Task {
		public override string Name {
			get {
				return "Detecting Visual Studio installation";
			}
		}

		public override void Execute() {
			var xKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", false);
			bool xFullVSInstalled = xKey != null;
			xKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VCSExpress\9.0", false);
			bool xVCSExpressInstalled = xKey != null;
			if (xFullVSInstalled && xVCSExpressInstalled) {
				ChooseVSWindow xChoose = new ChooseVSWindow();
				bool? xResult = xChoose.ShowDialog();
				if (!(xResult.HasValue && xResult.Value)) {
					throw new Exception("The installation has been canceled");
				}
				xFullVSInstalled = xChoose.cbxVisualStudio.IsChecked.HasValue ? xChoose.cbxVisualStudio.IsChecked.Value : false;
				xVCSExpressInstalled = xChoose.cbxVCSExpress.IsChecked.HasValue ? xChoose.cbxVCSExpress.IsChecked.Value : false;
			}
			if(!(xFullVSInstalled || xVCSExpressInstalled)) {
				throw new Exception("No Visual Studio Installation found!");
			}
			if (xVCSExpressInstalled) {
				Tools.VCSPath = (string)xKey.GetValue("InstallDir");
				Tools.VCSTemplatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Visual Studio 2008\Templates\ProjectTemplates\Visual C#");
			}
			if (xFullVSInstalled) {
				xKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", false);
				Tools.VSPath = xKey.GetValue("InstallDir") as string;
				xKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", false);
				Tools.VSTemplatePath = Path.Combine((string)xKey.GetValue("UserProjectTemplatesLocation"), "Visual C#");
			}
			System.Windows.MessageBox.Show("VCS path = '" + Tools.VCSPath + "'");
			System.Windows.MessageBox.Show("VS path = '" + Tools.VSPath + "'");
				
		}
	}
}