using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.X86.Win32 {
	public class Win32OpCodeMap: X86OpCodeMap {
		protected override Type GetCustomMethodImplementationOp() {
			return typeof(Win32CustomMethodImplementationOp);
		}

		protected override IList<AssemblyDefinition> GetPlugAssemblies() {
			IList<AssemblyDefinition> xResult = base.GetPlugAssemblies();
			xResult.Add(AssemblyFactory.GetAssembly(typeof (Win32OpCodeMap).Assembly.Location));
			return xResult;
		}

//		public override MethodReference GetCustomMethodImplementation_Old(string aOrigMethodName, bool aInMetalMode) {
//			switch (aOrigMethodName) {
//				case "System_Void___System_Console_Write___System_String___": {
//						return CustomImplementations.System.ConsoleImplRefs.WriteRef;
//					}
//				case "System_Void___System_Console_WriteLine____": {
//						return CustomImplementations.System.ConsoleImplRefs.WriteLineRef;
//					}
//				case "System_Void___System_Console_WriteLine___System_String___": {
//						return CustomImplementations.System.ConsoleImplRefs.WriteLine_string_Ref;
//					}
//				default:
//					return base.GetCustomMethodImplementation_Old(aOrigMethodName, aInMetalMode);
//			}
//		}
	}
}
