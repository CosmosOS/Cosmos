using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel.Boot.Glue;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.IL.X86.Native.CustomImplementations.System;
using Indy.IL2CPU.IL.X86.Native.CustomImplementations.System.Diagnostics;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86.Native;

namespace Indy.IL2CPU.IL.X86.Native {
	public class NativeOpCodeMap: X86.X86OpCodeMap {
		internal static NativeOpCodeMap Instance;
		public NativeOpCodeMap() {
			Instance = this;
		}

		public MethodDefinition GetGlueMethod(GlueMethodTypeEnum aMethodType) {
			CheckGlueMethod();
			if (!mGlueMethods.ContainsKey(aMethodType)) {
				throw new Exception("GlueMethod '" + aMethodType.ToString() + "' not implemented!");
			}
			return mGlueMethods[aMethodType];
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

		private SortedList<GlueMethodTypeEnum, MethodDefinition> mGlueMethods;

		private void CheckGlueMethod() {
			if (mGlueMethods != null) {
				return;
			}
			mGlueMethods = new SortedList<GlueMethodTypeEnum, MethodDefinition>();
			AssemblyDefinition xCrawledAsm = Engine.GetCrawledAssembly();
			foreach (ModuleDefinition xModule in xCrawledAsm.Modules) {
				foreach (TypeDefinition xType in xModule.Types) {
					foreach (MethodDefinition xMethod in xType.Methods) {
						CustomAttribute xAttrib = (from item in xMethod.CustomAttributes.Cast<CustomAttribute>()
												   where item.Constructor.DeclaringType.FullName == typeof(GlueMethodAttribute).FullName
												   select item).FirstOrDefault();
						if (xAttrib == null) {
							continue;
						}
						if (!xAttrib.Resolved) {
							if (!xAttrib.Resolve()) {
								throw new Exception("Couldn't resolve attribute on method '" + xMethod.GetFullName());
							}
						}
						mGlueMethods.Add((GlueMethodTypeEnum)xAttrib.Properties["MethodType"], xMethod);
					}
				}
			}
		}

		public override MethodReference GetCustomMethodImplementation(string aOrigMethodName, bool aInMetalMode) {
			switch (aOrigMethodName) {
				case "System_Void___System_Console_Clear____": {
						return ConsoleImplRefs.ClearRef;
					}
				case "System_Void___System_Console_WriteLine___System_String___": {
						return ConsoleImplRefs.WriteLineRef;
					}
				case "System_Void___System_Console_Write___System_String___": {
						return ConsoleImplRefs.Write_string_Ref;
					}
				case "System_Void___System_Console_Write___System_Char___": {
						return ConsoleImplRefs.Write_char_Ref;
					}
				case "System_Void___System_Diagnostics_Debug_WriteLine___System_String___": {
						return DebugImplRefs.WriteLineRef;
					}
				case "System_Void___System_Diagnostics_Debug_WriteLineIf___System_Boolean__System_String___": {
						return DebugImplRefs.WriteLineIfRef;
					}
				case "System_Void___Indy_IL2CPU_RuntimeEngine_InitializeEngine____": {
						return RuntimeEngineImplRefs.InitializeEngineRef;
					}
				case "System_Void___Indy_IL2CPU_RuntimeEngine_FinalizeEngine____": {
						return RuntimeEngineImplRefs.FinalizeEngineRef;
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
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_IDT_LoadArray____": {
						DoAssemble_IDT_LoadArray(aAssembler, aMethodInfo);
						return;
					}
				case "System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____": {
						return;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_IDT_RegisterIDT____": {
						DoAssemble_IDT_RegisterIDT(aAssembler, aMethodInfo);
						return;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_GDT_LoadArray____": {
						DoAssemble_GDT_LoadArray(aAssembler, aMethodInfo);
						return;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_GDT_RegisterGDT____": {
						DoAssemble_GDT_RegisterGDT(aAssembler, aMethodInfo);
						return;
					}
				case "System_Void___System_Diagnostics_Debugger_Break____": {
						//aAssembler.Add(new Literal("xchg bx, bx"));
						return;
					}
				case "System_Void___Cosmos_Kernel_ConsoleDrv_TestIDT____": {
						aAssembler.Add(new Literal("xchg bx, bx"));
						aAssembler.Add(new Literal("int 3"));
						break;
					}
				case "System_Void___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_IO_WriteToPort___System_UInt16__System_Byte__": {
						IL.X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
						aAssembler.Add(new CPU.Pop("eax"));
						IL.X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[1].VirtualAddresses, aMethodInfo.Arguments[1].Size);
						aAssembler.Add(new CPU.Pop("ecx"));
						aAssembler.Add(new CPUNative.Out("ax", "cl"));
						break;
					}
				case "System_Byte___Indy_IL2CPU_IL_X86_Native_RuntimeEngineImpl_IO_ReadFromPort___System_UInt16__": {
						IL.X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
						aAssembler.Add(new CPU.Pop("ecx"));
						aAssembler.Add(new CPU.Move("eax", "0"));
						aAssembler.Add(new CPUNative.In("ecx", "al"));
						break;
					}
			}
			base.DoCustomAssembleImplementation(aMethodName, aInMetalMode, aAssembler, aMethodInfo);
		}

		private void DoAssemble_IDT_RegisterIDT(Assembler.Assembler aAssembler, MethodInformation aInfo) {
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			FieldDefinition xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mIDTPointer");
			string xPointerFieldName;
			Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
			MethodDefinition xTheMethod = Engine.GetMethodDefinition(xRuntimeEngineTypeDef, "IDT_SetHandler", "System.Byte", "System.UInt32", "System.UInt16", xRuntimeEngineTypeDef.FullName + "/IDTEntryStruct/FlagsEnum");
			Engine.QueueMethod(xTheMethod);
			mIDTSetHandlerMethodName = new Label(xTheMethod).Name;
			aAssembler.Add(new CPU.Call("___________REGISTER___ISRS_____"));
			aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
			//aAssembler.Add(new Literal("XCHG BX, BX "));
			aAssembler.Add(new CPUNative.Cli());
			aAssembler.Add(new CPUNative.Lidt("eax"));
			aAssembler.Add(new CPUNative.Sti());
		}

		private void DoAssemble_GDT_RegisterGDT(Assembler.Assembler aAssembler, MethodInformation aInfo) {
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			FieldDefinition xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mGDTPointer");
			string xPointerFieldName;
			Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
			aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
			//aAssembler.Add(new Literal("XCHG BX, BX "));
			aAssembler.Add(new CPUNative.Cli());
			aAssembler.Add(new CPUNative.Lgdt("eax"));
			aAssembler.Add(new CPU.Move("eax", "0x8"));
			//aAssembler.Add(new CPU.Move("ds", "ax"));
			//aAssembler.Add(new CPU.Move("es", "ax"));
			//aAssembler.Add(new CPU.Move("fs", "ax"));
			//aAssembler.Add(new CPU.Move("gs", "ax"));
			//aAssembler.Add(new CPU.Move("ss", "ax"));
			aAssembler.Add(new CPU.JumpAlways("0x0008:flush____gdt______table"));
			aAssembler.Add(new Label("flush____gdt______table"));
			//aAssembler.Add(new CPUNative.Sti());
		}


		private string mIDTSetHandlerMethodName;

		private void DoAssemble_GDT_LoadArray(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			FieldDefinition xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mGDTEntries");
			string xFieldName = Assembler.DataMember.GetStaticFieldName(xFieldDef);
			string xFieldData = "0,0,0,0,2,0,0,0,1,0,0,0";
			for (int i = 0; i < 3; i++) {
				xFieldData += ",0,0,0,0,0,0,0,0";
			}
			aAssembler.DataMembers.RemoveAll(delegate(DataMember aItem) {
				return aItem.Name == xFieldName;
			});
			aAssembler.DataMembers.Add(new DataMember(xFieldName, "dd", xFieldName));
			aAssembler.DataMembers.Add(new DataMember(xFieldName + "___Contents", "db", xFieldData));
			xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mGDTPointer");
			string xPointerFieldName;
			Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
			aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
			aAssembler.Add(new CPU.Move("word [eax]", "0x" + ((8 * 3) - 1).ToString("X")));
			aAssembler.Add(new CPU.Move("ecx", xFieldName));
			aAssembler.Add(new CPU.Add("ecx", "0xC"));
			aAssembler.Add(new CPU.Move("dword [eax + 2]", "ecx"));
		}

		private void DoAssemble_IDT_LoadArray(Indy.IL2CPU.Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			//aAssembler.Add(new Literal("XCHG BX, BX "));
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			MethodDefinition xTheMethod = Engine.GetMethodDefinition(xRuntimeEngineTypeDef, "IDT_SetHandler", "System.Byte", "System.UInt32", "System.UInt16", xRuntimeEngineTypeDef.FullName + "/IDTEntryStruct/FlagsEnum");
			Engine.QueueMethod(xTheMethod);
			mIDTSetHandlerMethodName = new Label(xTheMethod).Name;
			FieldDefinition xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mIDTEntries");
			string xFieldName = Assembler.DataMember.GetStaticFieldName(xFieldDef);
			string xFieldData = "0,0,0,0,2,0,0,0,1,0,0,0";
			for (int i = 0; i < 256; i++) {
				xFieldData += ",0,0,0,0,0,0,0,0";
			}
			aAssembler.DataMembers.RemoveAll(delegate(DataMember aItem) {
				return aItem.Name == xFieldName;
			});
			aAssembler.DataMembers.Add(new DataMember(xFieldName, "dd", xFieldName + "___Contents"));
			aAssembler.DataMembers.Add(new DataMember(xFieldName + "___Contents", "db", xFieldData));
			xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mIDTPointer");
			string xPointerFieldName;
			Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
			aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
			aAssembler.Add(new CPU.Move("word [eax]", "0x" + ((8 * 256) - 1).ToString("X")));
			aAssembler.Add(new CPU.Move("ecx", xFieldName));
			aAssembler.Add(new CPU.Add("ecx", "0xC"));
			aAssembler.Add(new CPU.Move("dword [eax + 2]", "ecx"));
		}

		private static void DoAssemble_String_GetByteFromChar(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
		}

		public override void PostProcess(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			base.PostProcess(aAssembler);
			return;
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			MethodDefinition xTheMethod = Engine.GetMethodDefinition(xRuntimeEngineTypeDef, "IDT_SetHandler", "System.Byte", "System.UInt32", "System.UInt16", xRuntimeEngineTypeDef.FullName + "/IDTEntryStruct/FlagsEnum");
			Engine.QueueMethod(xTheMethod);
			mIDTSetHandlerMethodName = new Label(xTheMethod).Name;
			X86.X86MethodHeaderOp.AssembleHeader(aAssembler, "___________REGISTER___ISRS_____", new int[0]);
			string xInterruptHandlerLabel = new Label(Engine.GetMethodDefinition(Engine.GetTypeDefinition("Indy.IL2CPU.IL.X86.Native", "Indy.IL2CPU.IL.X86.Native.RuntimeEngineImpl"), "InterruptHandler", "System.Byte", "System.Byte")).Name;
			int[] xInterruptsWithParam = new int[] { 8, 10, 11, 12, 13, 14 };
			for (int i = 0; i < 256; i++) {
				aAssembler.Add(new CPU.Push("0x" + i.ToString("X")));
				//if (i == 3) {
				//	aAssembler.Add(new Literal("xchg bx, bx"));
				//}
				aAssembler.Add(new CPU.Push("____INTERRUPT_HANDLER___" + i));
				aAssembler.Add(new CPU.Push("0x08"));
				aAssembler.Add(new CPU.Push("0x8E"));
				aAssembler.Add(new CPU.Call(mIDTSetHandlerMethodName));
			}
			X86.X86MethodFooterOp.AssembleFooter(0, aAssembler, new int[0], 0);
			for (int j = 0; j < 256; j++) {
				aAssembler.Add(new Label("____INTERRUPT_HANDLER___" + j));
				aAssembler.Add(new CPUNative.Cli());
				aAssembler.Add(new CPU.Push(j.ToString()));
				if (!xInterruptsWithParam.Contains(j)) {
					aAssembler.Add(new CPU.Push("0"));
				}
				aAssembler.Add(new CPU.JumpAlways("____INTERRUPT_HANDLER___GENERIC"));
			}
			aAssembler.Add(new Label("____INTERRUPT_HANDLER___GENERIC"));
			aAssembler.Add(new CPUNative.Pushad());
			aAssembler.Add(new CPU.Move("ax", "ds"));
			aAssembler.Add(new CPU.Push("eax"));
			aAssembler.Add(new CPU.Move("ds", "ax"));
			aAssembler.Add(new CPU.Move("es", "ax"));
			aAssembler.Add(new CPU.Move("fs", "ax"));
			aAssembler.Add(new CPU.Move("gs", "ax"));
			aAssembler.Add(new CPU.Call(xInterruptHandlerLabel));
			aAssembler.Add(new CPU.Pop("ebx"));
			aAssembler.Add(new CPU.Move("ds", "bx"));
			aAssembler.Add(new CPU.Move("es", "bx"));
			aAssembler.Add(new CPU.Move("fs", "bx"));
			aAssembler.Add(new CPU.Move("gs", "bx"));
			aAssembler.Add(new CPUNative.Popad());
			aAssembler.Add(new CPUNative.Sti());
			aAssembler.Add(new CPUNative.IRet());
		}
	}
}