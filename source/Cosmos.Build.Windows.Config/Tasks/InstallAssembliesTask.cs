using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace Cosmos.Build.Windows.Config.Tasks {
	public class InstallAssembliesTask: Task {
		public override string Name {
			get {
				return "Installing Cosmos Assemblies";
			}
		}

		public override void Execute() {
			string xBaseDir = Tools.Dir("GAC");
			string xTargetDir;
			OnStatus(0, "Installing Cosmos Assemblies");
			using (var xKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", false)) {
				xTargetDir = (string)xKey.GetValue("InstallDir");
				xTargetDir = Path.Combine(xTargetDir, "PublicAssemblies");
			}
			string[] xItems = Directory.GetFiles(xBaseDir);
			int xCurrent = 1;
			foreach (string xFile in Directory.GetFiles(xBaseDir)) {
				OnStatus(100 - (xItems.Length / xCurrent), "Copying " + Path.GetFileNameWithoutExtension(xFile));
				File.Copy(xFile, Path.Combine(xTargetDir, Path.GetFileName(xFile)), true);
				xCurrent++;
				OnStatus(100 - (xItems.Length / xCurrent), "Copying " + Path.GetFileNameWithoutExtension(xFile));
			}
			OnStatus(100, "Installing Cosmos Assemblies");
		}
	}
}