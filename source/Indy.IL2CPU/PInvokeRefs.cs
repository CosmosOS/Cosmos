using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class PInvokeRefs {
		public static readonly Mono.Cecil.MethodReference Kernel32_ExitProcess_Ref;
		static PInvokeRefs() {
			Mono.Cecil.AssemblyDefinition xAD = AssemblyFactory.GetAssembly(typeof (PInvokes).Assembly.Location);
			//xAD.Find
		}
	}
}