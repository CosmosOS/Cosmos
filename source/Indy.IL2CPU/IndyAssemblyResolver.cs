using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public class IndyAssemblyResolver: DefaultAssemblyResolver {
		private readonly AssemblyDefinition mCrawledAssembly;
		public IndyAssemblyResolver(AssemblyDefinition aCrawledAssembly) {
			mCrawledAssembly = aCrawledAssembly;
			List<AssemblyDefinition> xDefs = new List<AssemblyDefinition>();
			xDefs.Add(mCrawledAssembly);
			for(int i = 0; i<xDefs.Count; i++) {
				foreach(ModuleDefinition xModDef in xDefs[i].Modules) {
					foreach(AssemblyNameReference xAssemblyRef in  xModDef.AssemblyReferences) {
						AssemblyDefinition xFoundAsm = mCrawledAssembly.Resolver.Resolve(xAssemblyRef);
						if(xFoundAsm!=null&&!xDefs.Contains(xFoundAsm)) {
							xDefs.Add(xFoundAsm);
						}
					}
				}
			}
			foreach(AssemblyDefinition xAssembly in xDefs) {
				RegisterAssembly(xAssembly);
			}
		}

		public void RegisterAssembly(AssemblyDefinition aAssembly) {
			base.RegisterAssembly(aAssembly);
		}

		public void RegisterAssembly(string aFile) {
			base.RegisterAssembly(AssemblyFactory.GetAssembly(aFile));
		}
	}
}