using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.NativeX86.CustomImplementations.System;
using Indy.IL2CPU.IL.NativeX86.CustomImplementations.System.Diagnostics;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.NativeX86 {
	public class NativeX86OpCodeMap: X86.X86OpCodeMap {
		protected override Type GetCustomMethodImplementationOp() {
			return typeof(NativeX86CustomMethodImplementationOp);
		}

		public override MethodReference GetCustomMethodImplementation(string aOrigMethodName, bool aInMetalMode) {
			switch (aOrigMethodName) {
				case "System_Void___System_Console_Clear____": {
						return ConsoleImplRefs.ClearRef;
					}
				case "System_Void___System_Console_WriteLine___System_String___": {
						return ConsoleImplRefs.WriteLineRef;
					}
				case "System_Void___System_Diagnostics_Debugger_Break____": {
						return DebuggerImplRefs.BreakRef;
					}
				default:
					return base.GetCustomMethodImplementation(aOrigMethodName, aInMetalMode);
			}
		}

		public override bool HasCustomAssembleImplementation(string aMethodName, bool aInMetalMode) {
			switch (aMethodName) {
				case "System_Byte___Indy_IL2CPU_IL_X86_CustomImplementations_System_StringImpl_GetByteFromChar___System_Char___": {
						if (aInMetalMode) {
							return true;
						}
						break;
					}
			}
			return base.HasCustomAssembleImplementation(aMethodName, aInMetalMode);
		}

		public override void DoCustomAssembleImplementation(string aMethodName, bool aInMetalMode, Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			switch (aMethodName) {
				case "System_Byte___Indy_IL2CPU_IL_X86_CustomImplementations_System_StringImpl_GetByteFromChar___System_Char___": {
						if (aInMetalMode) {
							DoAssemble_String_GetByteFromChar(aAssembler, aMethodInfo);
							return;
						}
						break;
					}
			}
			base.DoCustomAssembleImplementation(aMethodName, aInMetalMode, aAssembler, aMethodInfo);
		}

		private static void DoAssemble_String_GetByteFromChar(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			Console.WriteLine("Assembling Custom GetByteFromChar");
			X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddress);
		}
	}
}
