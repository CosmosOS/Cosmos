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
			string xBaseDir = Tools.CosmosDir("GAC");
			List<string> xTargetPaths = new List<string>();
			if (!String.IsNullOrEmpty(Tools.VSPath)) {
				xTargetPaths.Add(Tools.VSPath);
			}
			if (!String.IsNullOrEmpty(Tools.VCSPath)) {
				xTargetPaths.Add(Tools.VCSPath);
			}
			OnStatus(0, "Installing Cosmos Assemblies");
			foreach (string xTargetBaseDir in xTargetPaths) {
				string xTargetDir = Path.Combine(xTargetBaseDir, "PublicAssemblies");
				string[] xItems = Directory.GetFiles(xBaseDir);
				int xCurrent = 1;
				foreach (string xFile in Directory.GetFiles(xBaseDir)) {
					OnStatus(100 - ((xItems.Length + 1) / xCurrent), "Copying " + Path.GetFileNameWithoutExtension(xFile));
					File.Copy(xFile, Path.Combine(xTargetDir, Path.GetFileName(xFile)), true);
					xCurrent++;
					OnStatus(100 - (xItems.Length / xCurrent), "Copying " + Path.GetFileNameWithoutExtension(xFile));
				}
			}
			var xKey = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos", true);
			if (xKey == null) {
				xKey = Registry.CurrentUser.CreateSubKey(@"Software\Cosmos");
			}
			xKey.SetValue("Build Path", Path.GetDirectoryName(typeof(InstallAssembliesTask).Assembly.Location));
			xKey.Flush();
			OnStatus(100, "Installing Cosmos Assemblies");
		}
	}
}