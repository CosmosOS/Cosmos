using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.IL.X86.Native.CustomImplementations.System;
using Indy.IL2CPU.IL.X86.Native.CustomImplementations.System.Diagnostics;

using CPU = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86.Native;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86.Native {
	public class NativeOpCodeMap: X86.X86OpCodeMap {
		internal static NativeOpCodeMap Instance;

		public override void Initialize(Indy.IL2CPU.Assembler.Assembler aAssembler, IEnumerable<Assembly> aProjectAssemblies) {
			base.Initialize(aAssembler, aProjectAssemblies);
			base.mMap[OpCodeEnum.Call] = typeof(Call);
		}

		public NativeOpCodeMap() {
			Instance = this;
		}

		protected override Type GetCustomMethodImplementationOp() {
			return typeof(NativeCustomMethodImplementationOp);
		}

		protected override Type GetMethodFooterOp() {
			return typeof(NativeMethodFooterOp);
		}

		protected override Type GetMainEntryPointOp() {
			return typeof(NativeMainEntryMethodOp);
		}

		protected override Type GetMethodHeaderOp() {
			return typeof(NativeMethodHeaderOp);
		}

		public override IList<Assembly> GetPlugAssemblies() {
			IList<Assembly> xResult = base.GetPlugAssemblies();
			xResult.Add(typeof(NativeOpCodeMap).Assembly);
			return xResult;
		}

		public override void EmitOpDebugHeader(Indy.IL2CPU.Assembler.Assembler aAssembler, uint aOpId, string aOpLabel, ushort aDebugComport) {
			base.EmitOpDebugHeader(aAssembler, aOpId, aOpLabel, aDebugComport);
			ushort xAddress = 0;
			switch (aDebugComport) {
				case 1:
					xAddress = 0x3F8;
					break;
				case 2:
					xAddress = 0x2F8;
					break;
				case 3:
					xAddress = 0x3E8;
					break;
				case 4:
					xAddress = 0x2E8;
					break;
				default:
					throw new Exception("Com " + aDebugComport + " not supported!");
			}
			new CPU.Call("DebugPoint__");
			//new CPUNative.InByte
			//int is_transmit_empty() {
			//   return inb(PORT + 5) & 0x20;
			//}

			//void write_serial(char a) {
			//   while (is_transmit_empty() == 0)
			//		;

			//   outb(PORT,a);
			//}


		}

		public override bool HasCustomAssembleImplementation(MethodInformation aMethodInfo, bool aInMetalMode) {
			switch (aMethodInfo.LabelName) {
				case "System_Byte___Indy_IL2CPU_IL_X86_CustomImplementations_System_StringImpl_GetByteFromChar___System_Char___": {
						if (aInMetalMode) {
							return true;
						}
						break;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_IDT_LoadArray____": {
						return true;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_IDT_RegisterIDT____": {
						return true;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_GDT_LoadArray____": {
						return true;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_GDT_RegisterGDT____": {
						return true;
					}
				case "System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____": {
						return true;
					}
				case "System_Void___System_Diagnostics_Debugger_Break____": {
						return true;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_IO_WriteToPort___System_UInt16__System_Byte__": {
						return true;
					}
				case "System_Byte___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_IO_ReadFromPort___System_UInt16__": {
						return true;
					}
				case "System_Void___Cosmos_Kernel_ConsoleDrv_TestIDT____": {
						return true;
					}
				default: {
						break;
					}
			}
			return base.HasCustomAssembleImplementation(aMethodInfo, aInMetalMode);
		}

		public override void DoCustomAssembleImplementation(bool aInMetalMode, Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			switch (aMethodInfo.LabelName) {
				case "System_Byte___Indy_IL2CPU_IL_X86_CustomImplementations_System_StringImpl_GetByteFromChar___System_Char___": {
						if (aInMetalMode) {
							DoAssemble_String_GetByteFromChar(aAssembler, aMethodInfo);
							return;
						}
						break;
					}
				case "System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____": {
						return;
					}
				case "System_Void___System_Diagnostics_Debugger_Break____": {
						//aAssembler.Add(new Literal("xchg bx, bx"));
						return;
					}
				default: {
						break;
					}
			}
			base.DoCustomAssembleImplementation(aInMetalMode, aAssembler, aMethodInfo);
		}

		private static void DoAssemble_String_GetByteFromChar(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
		}

		public override void PostProcess(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			base.PostProcess(aAssembler);
		}
	}
}