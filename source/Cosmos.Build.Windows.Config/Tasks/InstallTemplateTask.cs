using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;

namespace Cosmos.Build.Windows.Config.Tasks {
	public class InstallTemplateTask: Task {
		public override string Name {
			get {
				return "Install VS Template (C#)";
			}
		}

		public override void Execute() {
			this.OnStatus(0, "Installing Template");
			string xTemplateFile = Tools.Dir("CosmosBoot.zip");
			string xVSTemplateFolder;
			using (var xKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0")) {
				xVSTemplateFolder = (string)xKey.GetValue("UserProjectTemplatesLocation");
				xVSTemplateFolder = Path.Combine(xVSTemplateFolder, "Visual C#");
			}
			File.Copy(xTemplateFile, Path.Combine(xVSTemplateFolder, "CosmosBoot.zip"));
			this.OnStatus(100, "Installing Template");
		}
	}
}