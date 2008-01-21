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
			string xTemplateFile = Tools.CosmosDir("CosmosBoot.zip");
			List<string> xTargetPaths = new List<string>();
			if (!String.IsNullOrEmpty(Tools.VSTemplatePath)) {
				xTargetPaths.Add(Tools.VSTemplatePath);
			}
			if (!String.IsNullOrEmpty(Tools.VCSTemplatePath)) {
				xTargetPaths.Add(Tools.VCSTemplatePath);
			}
			foreach (string xTargetFolder in xTargetPaths) {
				File.Copy(xTemplateFile, Path.Combine(xTargetFolder, "CosmosBoot.zip"), true);
			}
			this.OnStatus(100, "Installing Template");
		}
	}
}