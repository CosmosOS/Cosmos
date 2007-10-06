using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.IL.NativeX86.CustomImplementations.System;
using Indy.IL2CPU.IL.NativeX86.CustomImplementations.System.Diagnostics;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.NativeX86;

namespace Indy.IL2CPU.IL.NativeX86 {
	public class NativeX86OpCodeMap: X86.X86OpCodeMap {
		protected override Type GetCustomMethodImplementationOp() {
			return typeof(NativeX86CustomMethodImplementationOp);
		}

		protected override Type GetMethodHeaderOp() {
			return typeof(NativeX86MethodHeaderOp);
		}

		public override MethodReference GetCustomMethodImplementation(string aOrigMethodName, bool aInMetalMode) {
			switch (aOrigMethodName) {
				case "System_Void___System_Console_Clear____": {
						return ConsoleImplRefs.ClearRef;
					}
				case "System_Void___System_Console_WriteLine___System_String___": {
						return ConsoleImplRefs.WriteLineRef;
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
				case "System_Void___Indy_IL2CPU_IL_NativeX86_RuntimeEngineImpl_IDT_LoadArray____": {
						return true;
					}
				case "System_Void___Indy_IL2CPU_IL_NativeX86_RuntimeEngineImpl_IDT_RegisterIDT____": {
						return true;
					}
				case "System_Void___Indy_IL2CPU_IL_NativeX86_RuntimeEngineImpl_GDT_LoadArray____": {
						return true;
					}
				case "System_Void___Indy_IL2CPU_IL_NativeX86_RuntimeEngineImpl_GDT_RegisterGDT____": {
						return true;
					}
				case "System_Void___System_Diagnostics_Debugger_Break____": {
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
				case "System_Void___Indy_IL2CPU_IL_NativeX86_RuntimeEngineImpl_IDT_LoadArray____": {
						DoAssemble_IDT_LoadArray(aAssembler, aMethodInfo);
						return;
					}
				case "System_Void___Indy_IL2CPU_IL_NativeX86_RuntimeEngineImpl_IDT_RegisterIDT____": {
						DoAssemble_IDT_RegisterIDT(aAssembler, aMethodInfo);
						return;
					}
				case "System_Void___Indy_IL2CPU_IL_NativeX86_RuntimeEngineImpl_GDT_LoadArray____": {
						DoAssemble_GDT_LoadArray(aAssembler, aMethodInfo);
						return;
					}
				case "System_Void___Indy_IL2CPU_IL_NativeX86_RuntimeEngineImpl_GDT_RegisterGDT____": {
						DoAssemble_GDT_RegisterGDT(aAssembler, aMethodInfo);
						return;
					}
				case "System_Void___System_Diagnostics_Debugger_Break____": {
						aAssembler.Add(new Literal("xchg bx, bx"));
						return;
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
			aAssembler.Add(new CPUNative.Lidt("eax"));
		}

		private string mIDTSetHandlerMethodName;

		private void DoAssemble_IDT_LoadArray(Indy.IL2CPU.Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			//aAssembler.Add(new Literal("XCHG BX, BX "));
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			FieldDefinition xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mIDTEntries");
			string xFieldName = Assembler.DataMember.GetStaticFieldName(xFieldDef);
			string xFieldData = "0,0,0,0,2,0,0,0,0,0,0,0";
			for (int i = 0; i < 256; i++) {
				xFieldData += ",0,0,0,0,0,0,0,0";
			}
			aAssembler.DataMembers.RemoveAll(delegate(DataMember aItem) {
				return aItem.Name == xFieldName;
			});
			aAssembler.DataMembers.Add(new DataMember(xFieldName, "db", xFieldData));
			xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mIDTPointer");
			string xPointerFieldName;
			Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
			aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
			aAssembler.Add(new CPU.Move("word [eax]", "0x" + ((8 * 1) - 1).ToString("X")));
			aAssembler.Add(new CPU.Move("dword [eax + 2]", xFieldName));
		}

		private static void DoAssemble_String_GetByteFromChar(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddress);
		}

		public override void PostProcess(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			base.PostProcess(aAssembler);
			X86.X86MethodHeaderOp.AssembleHeader(aAssembler, "___________REGISTER___ISRS_____", 0);
			for (int i = 0; i < aAssembler.Instructions.Count; i++) {
				Label xLbl = aAssembler.Instructions[i] as Label;
				if (xLbl == null) {
					continue;
				}
				if (!xLbl.Name.EndsWith(NativeX86MethodHeaderOp.ISR_Suffix)) {
					continue;
				}
				Literal xLiteral = aAssembler.Instructions[i + 1] as Literal;
				if (xLiteral == null)
					continue;
				byte theNum;
				if (!Byte.TryParse(xLiteral.Data.Substring(1), out theNum)) {
					continue;
				}
				aAssembler.Add(new CPU.Push("0x" + theNum.ToString("X")));
				aAssembler.Add(new CPU.Push(xLbl.Name));
				aAssembler.Add(new CPU.Push("0x08"));
				aAssembler.Add(new CPU.Push("0x8E"));
				aAssembler.Add(new CPU.Call(mIDTSetHandlerMethodName));
			}
			X86.X86MethodFooterOp.AssembleFooter(false, aAssembler, 0, 0);
		}

		private void DoAssemble_GDT_RegisterGDT(Assembler.Assembler aAssembler, MethodInformation aInfo) {
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			FieldDefinition xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mGDTPointer");
			string xPointerFieldName;
			Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
			//aAssembler.Add(new Literal("XCHG BX, BX "));
			aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
			aAssembler.Add(new CPUNative.Lgdt("eax"));
			aAssembler.Add(new CPU.Move("ax", "0x10"));
			aAssembler.Add(new CPU.Move("ds", "ax"));
			aAssembler.Add(new CPU.Move("es", "ax"));
			aAssembler.Add(new CPU.Move("fs", "ax"));
			aAssembler.Add(new CPU.Move("gs", "ax"));
			aAssembler.Add(new CPU.Move("ss", "ax"));
			aAssembler.Add(new CPU.JumpAlways("0x8:___GDT_REGISTERGDT____FLUSH2"));
			aAssembler.Add(new Label("___GDT_REGISTERGDT____FLUSH2"));

		}

		private void DoAssemble_GDT_LoadArray(Indy.IL2CPU.Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			string xFieldName = "_______GDT____ENTRIES____";
			string xFieldData = "0,0,2,0,";
			// system segment
			xFieldData += "0,0,0,0,";
			//  code
			xFieldData += "1111111111111111b,";
			xFieldData += "0000000000000000b,";
			xFieldData += "0000000010011010b,";
			xFieldData += "1100111100000000b,";
			// data
			xFieldData += "1111111111111111b,";
			xFieldData += "0000000000000000b,";
			xFieldData += "0000000010010010b,";
			xFieldData += "1100111100000000b";
			aAssembler.DataMembers.RemoveAll(delegate(DataMember aItem) {
				return aItem.Name == xFieldName;
			});
			aAssembler.DataMembers.Add(new DataMember(xFieldName, "dw", xFieldData));
			FieldDefinition xFieldDef = xRuntimeEngineTypeDef.Fields.GetField("mGDTPointer");
			string xPointerFieldName;
			Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
			aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
			//aAssembler.Add(new Literal("XCHG BX, BX "));
			aAssembler.Add(new CPU.Move("word [eax]", "0x" + ((8 * 3) - 1).ToString("X")));
			aAssembler.Add(new CPU.Move("dword [eax + 2]", xFieldName));
		}
	}
}
