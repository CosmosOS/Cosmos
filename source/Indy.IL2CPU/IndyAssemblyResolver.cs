using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public class IndyAssemblyResolver: DefaultAssemblyResolver {
		public void RegisterAssembly(string aFile) {
			base.RegisterAssembly(AssemblyFactory.GetAssembly(aFile));
		}
	}
}