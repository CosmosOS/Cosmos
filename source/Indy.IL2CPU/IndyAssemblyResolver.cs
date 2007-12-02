using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public class IndyAssemblyResolver: DefaultAssemblyResolver {
		private readonly AssemblyDefinition mCrawledAssembly;
		private string[] mSearchDirs;
		public IndyAssemblyResolver(AssemblyDefinition aCrawledAssembly, string[] aSearchDirs) {
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			mCrawledAssembly = aCrawledAssembly;
			mSearchDirs = aSearchDirs;
			aCrawledAssembly.Resolver = this;
			AddSearchDirectory(".");
			AddSearchDirectory("bin");
			List<string> xSearchDirs = new List<string>();
			foreach (string s in aSearchDirs) {
				if (!String.IsNullOrEmpty(s)) {
					string xRealPath = new DirectoryInfo(s).FullName;
					xSearchDirs.Add(xRealPath);
					AddSearchDirectory(xRealPath);
					AppDomain.CurrentDomain.AppendPrivatePath(xRealPath);
				}
			}
			mSearchDirs = xSearchDirs.ToArray();
			List<AssemblyDefinition> xDefs = new List<AssemblyDefinition>();
			xDefs.Add(mCrawledAssembly);
			for (int i = 0; i < xDefs.Count; i++) {
				foreach (ModuleDefinition xModDef in xDefs[i].Modules) {
					foreach (AssemblyNameReference xAssemblyRef in xModDef.AssemblyReferences) {
						AssemblyDefinition xFoundAsm = mCrawledAssembly.Resolver.Resolve(xAssemblyRef);
						if (xFoundAsm != null && !xDefs.Contains(xFoundAsm)) {
							xDefs.Add(xFoundAsm);
						}
					}
				}
			}
			foreach (AssemblyDefinition xAssembly in xDefs) {
				RegisterAssembly(xAssembly);
			}
		}

		Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
			string xAsmFile = args.Name;
			if (xAsmFile.Contains(",")) {
				xAsmFile = xAsmFile.Substring(0, xAsmFile.IndexOf(','));
			}
			foreach (string s in mSearchDirs) {
				if(File.Exists(Path.Combine(s, xAsmFile + ".dll"))) {
					return Assembly.LoadFile(Path.Combine(s, xAsmFile + ".dll"));
				}
				if (File.Exists(Path.Combine(s, xAsmFile + ".exe"))) {
					return Assembly.LoadFile(Path.Combine(s, xAsmFile + ".exe"));
				}
			}
			Console.Write("");
			return null;
		}

		public new void RegisterAssembly(AssemblyDefinition aAssembly) {
			base.RegisterAssembly(aAssembly);
		}

		public void RegisterAssembly(string aFile) {
			base.RegisterAssembly(AssemblyFactory.GetAssembly(aFile));
		}
	}
}