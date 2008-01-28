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

            bool bit64 = IntPtr.Size == 8;
            string concat = bit64 ? "\\Wow6432Node" : "";

			var xKey = Registry.LocalMachine.OpenSubKey(@"Software" + concat + @"\Microsoft\VisualStudio\9.0", false);
			var xVSPath = xKey == null ? "" : xKey.GetValue("InstallDir") as string;
            xKey = Registry.CurrentUser.OpenSubKey(@"Software" + concat + @"\Microsoft\VisualStudio\9.0", false);
			var xVSTemplatePath = xKey == null ? "" : String.IsNullOrEmpty(xKey.GetValue("UserProjectTemplatesLocation") as string) ? String.Empty : Path.Combine((string)xKey.GetValue("UserProjectTemplatesLocation"), "Visual C#");
			bool xFullVSInstalled = !(String.IsNullOrEmpty(xVSPath) || string.IsNullOrEmpty(xVSTemplatePath));
			if (xFullVSInstalled) {
				xFullVSInstalled &= File.Exists(Path.Combine(xVSPath, "devenv.exe"));
			}

            xKey = Registry.LocalMachine.OpenSubKey(@"Software" + concat + @"\Microsoft\VCSExpress\9.0", false);
			var xVCSPath = xKey == null ? "" : (string)xKey.GetValue("InstallDir");
			var xVCSTemplatePath = xKey == null ? "" : String.IsNullOrEmpty(xKey.GetValue("UserProjectTemplatesLocation") as string) ? String.Empty : Path.Combine((string)xKey.GetValue("UserProjectTemplatesLocation"), "Visual C#");

			bool xVCSExpressInstalled = !(String.IsNullOrEmpty(xVCSPath) || string.IsNullOrEmpty(xVCSTemplatePath));
			if (xVCSExpressInstalled) {
				xVCSExpressInstalled = File.Exists(Path.Combine(xVCSPath, "vcsexpress.exe"));
			}
			if (xFullVSInstalled && xVCSExpressInstalled) {
				ChooseVSWindow xChoose = new ChooseVSWindow();
				bool? xResult = xChoose.ShowDialog();
				if (!(xResult.HasValue && xResult.Value)) {
					throw new Exception("The installation has been canceled");
				}
				xFullVSInstalled = xChoose.cbxVisualStudio.IsChecked.HasValue ? xChoose.cbxVisualStudio.IsChecked.Value : false;
				xVCSExpressInstalled = xChoose.cbxVCSExpress.IsChecked.HasValue ? xChoose.cbxVCSExpress.IsChecked.Value : false;
			}
			if (!(xFullVSInstalled || xVCSExpressInstalled)) {
				throw new Exception("No Visual Studio Installation found!");
			}
			if (xVCSExpressInstalled) {
				Tools.VCSPath = xVCSPath;
				Tools.VCSTemplatePath = xVCSTemplatePath;
			}
			if (xFullVSInstalled) {
				Tools.VSPath = xVSPath;
				Tools.VSTemplatePath = xVSTemplatePath;
			}

		}
	}
}