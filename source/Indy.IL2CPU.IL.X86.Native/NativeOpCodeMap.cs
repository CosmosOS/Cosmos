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
			MethodDefinition xResult = mGlueMethods[aMethodType];
			Engine.QueueMethod(xResult);
			return xResult;
		}

		public MethodDefinition GetGluePlaceholderMethod(GluePlaceholderMethodTypeEnum aMethodType) {
			CheckGluePlaceholderMethod();
			if (!mGluePlaceholderMethods.ContainsKey(aMethodType)) {
				throw new Exception("GluePlaceholderMethod '" + aMethodType.ToString() + "' not implemented!");
			}
			return mGluePlaceholderMethods[aMethodType];
		}

		public FieldDefinition GetGlueField(GlueFieldTypeEnum aFieldType) {
			CheckGlueField();
			if (!mGlueFields.ContainsKey(aFieldType)) {
				throw new Exception("GlueField '" + aFieldType.ToString() + "' not found!");
			}
			return mGlueFields[aFieldType];
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
		private SortedList<GluePlaceholderMethodTypeEnum, MethodDefinition> mGluePlaceholderMethods;
		private SortedList<GlueFieldTypeEnum, FieldDefinition> mGlueFields;

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

		private void CheckGluePlaceholderMethod() {
			if (mGluePlaceholderMethods != null) {
				return;
			}
			mGluePlaceholderMethods = new SortedList<GluePlaceholderMethodTypeEnum, MethodDefinition>();
			AssemblyDefinition xCrawledAsm = Engine.GetCrawledAssembly();
			foreach (ModuleDefinition xModule in xCrawledAsm.Modules) {
				foreach (TypeDefinition xType in xModule.Types) {
					foreach (MethodDefinition xMethod in xType.Methods) {
						CustomAttribute xAttrib = (from item in xMethod.CustomAttributes.Cast<CustomAttribute>()
												   where item.Constructor.DeclaringType.FullName == typeof(GluePlaceholderMethodAttribute).FullName
												   select item).FirstOrDefault();
						if (xAttrib == null) {
							continue;
						}
						if (!xAttrib.Resolved) {
							if (!xAttrib.Resolve()) {
								throw new Exception("Couldn't resolve attribute on method '" + xMethod.GetFullName());
							}
						}
						mGluePlaceholderMethods.Add((GluePlaceholderMethodTypeEnum)xAttrib.Properties["MethodType"], xMethod);
					}
				}
			}
		}

		private void CheckGlueField() {
			if (mGlueFields != null) {
				return;
			}
			mGlueFields = new SortedList<GlueFieldTypeEnum, FieldDefinition>();
			AssemblyDefinition xCrawledAsm = Engine.GetCrawledAssembly();
			foreach (ModuleDefinition xModule in xCrawledAsm.Modules) {
				foreach (TypeDefinition xType in xModule.Types) {
					foreach (FieldDefinition xFieldDefinition in xType.Fields) {
						CustomAttribute xAttrib = (from item in xFieldDefinition.CustomAttributes.Cast<CustomAttribute>()
												   where item.Constructor.DeclaringType.FullName == typeof(GlueFieldAttribute).FullName
												   select item).FirstOrDefault();
						if (xAttrib == null) {
							continue;
						}
						if (!xAttrib.Resolved) {
							if (!xAttrib.Resolve()) {
								throw new Exception("Couldn't resolve attribute on method '" + xFieldDefinition.GetFullName());
							}
						}
						if (mGlueFields.ContainsKey((GlueFieldTypeEnum)xAttrib.Properties["FieldType"])) {
							System.Diagnostics.Debugger.Break();
							throw new Exception("GlueField of type '" + ((GlueFieldTypeEnum)xAttrib.Properties["FieldType"]).ToString() + "' already found!");
						}
						mGlueFields.Add((GlueFieldTypeEnum)xAttrib.Properties["FieldType"], xFieldDefinition);
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
						return GetGlueMethod(GlueMethodTypeEnum.Debug_WriteLine);
					}
				case "System_Void___System_Diagnostics_Debug_Write___System_String___": {
						return GetGlueMethod(GlueMethodTypeEnum.Debug_Write);
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
				case "System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___": {
						return GetGlueMethod(GlueMethodTypeEnum.Heap_MemAlloc);
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
				default: {
						CheckGluePlaceholderMethod();
						GluePlaceholderMethodTypeEnum? xMethodType = null;
						return (from item in mGluePlaceholderMethods.Keys
								where new Label(mGluePlaceholderMethods[item]).Name == aMethodName
								select item).Count() > 0;
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
				case "System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____": {
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
				default: {
						CheckGluePlaceholderMethod();
						GluePlaceholderMethodTypeEnum? xMethodType = null;

						foreach (GluePlaceholderMethodTypeEnum xTheMethodType in mGluePlaceholderMethods.Keys) {
							if (new Label(mGluePlaceholderMethods[xTheMethodType]).Name == aMethodName) {
								xMethodType = xTheMethodType;
								break;
							}
						}
						if (xMethodType != null) {
							AssembleGluePlaceholderMethod(xMethodType.Value, aAssembler, aMethodInfo);
						}
						break;
					}
			}
			base.DoCustomAssembleImplementation(aMethodName, aInMetalMode, aAssembler, aMethodInfo);
		}

		private void AssembleGluePlaceholderMethod(GluePlaceholderMethodTypeEnum aMethodType, Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			switch (aMethodType) {
				case GluePlaceholderMethodTypeEnum.GDT_LoadArray: {
						FieldDefinition xFieldDef = GetGlueField(GlueFieldTypeEnum.GDT_Array);
						string xFieldName = Assembler.DataMember.GetStaticFieldName(xFieldDef);
						string xFieldData = "0,0,0,0,2,0,0,0,4,0,0,0";
						for (int i = 0; i < 3; i++) {
							xFieldData += ",0,0,0,0,0,0,0,0";
						}
						DataMember xDataItem = (from item in aAssembler.DataMembers
												where item.Name == xFieldName
												select item).FirstOrDefault();
						aAssembler.DataMembers.Remove(xDataItem);
						aAssembler.DataMembers.Add(new DataMember(xFieldName, "dd", xFieldName));
						aAssembler.DataMembers.Add(new DataMember(xFieldName + "___Contents", "db", xFieldData));
						xFieldDef = GetGlueField(GlueFieldTypeEnum.GDT_Pointer);
						string xPointerFieldName;
						Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
						aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
						aAssembler.Add(new CPU.Move("word [eax]", "0x" + ((8 * 3) - 1).ToString("X")));
						aAssembler.Add(new CPU.Move("ecx", xFieldName));
						aAssembler.Add(new CPU.Add("ecx", "0xC"));
						aAssembler.Add(new CPU.Move("dword [eax + 2]", "ecx"));
						break;
					}
				case GluePlaceholderMethodTypeEnum.GDT_Register: {
						FieldDefinition xFieldDef = GetGlueField(GlueFieldTypeEnum.GDT_Pointer);
						string xPointerFieldName;
						Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
						aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
						aAssembler.Add(new CPUNative.Lgdt("[eax]"));
						aAssembler.Add(new Literal("use32"));
						aAssembler.Add(new CPU.Move("eax", "0x10"));
						aAssembler.Add(new CPU.Move("ds", "ax"));
						aAssembler.Add(new CPU.Move("es", "ax"));
						aAssembler.Add(new CPU.Move("fs", "ax"));
						aAssembler.Add(new CPU.Move("gs", "ax"));
						aAssembler.Add(new CPU.Move("ss", "ax"));
						aAssembler.Add(new CPU.JumpAlways("0x8:flush____gdt______table__part1"));
						aAssembler.Add(new Label("flush____gdt______table__part1"));
						break;
					}
				case GluePlaceholderMethodTypeEnum.IDT_LoadArray: {
						MethodDefinition xTheMethod = GetGlueMethod(GlueMethodTypeEnum.IDT_SetHandler);
						Engine.QueueMethod(xTheMethod);
						mIDTSetHandlerMethodName = new Label(xTheMethod).Name;
						FieldDefinition xFieldDef = GetGlueField(GlueFieldTypeEnum.IDT_Array);
						string xFieldName = Assembler.DataMember.GetStaticFieldName(xFieldDef);
						string xFieldData = "0,0,0,0,2,0,0,0,1,0,0,0";
						for (int i = 0; i < 256; i++) {
							xFieldData += ",0,0,0,0,0,0,0,0";
						}
						DataMember xDataItem = (from item in aAssembler.DataMembers
												where item.Name == xFieldName
												select item).FirstOrDefault();
						aAssembler.DataMembers.Remove(xDataItem);
						aAssembler.DataMembers.Add(new DataMember(xFieldName, "dd", xFieldName + "___Contents"));
						aAssembler.DataMembers.Add(new DataMember(xFieldName + "___Contents", "db", xFieldData));
						xFieldDef = GetGlueField(GlueFieldTypeEnum.IDT_Pointer);
						string xPointerFieldName;
						Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
						aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
						aAssembler.Add(new CPU.Move("word [eax]", "0x" + ((8 * 256) - 1).ToString("X")));
						aAssembler.Add(new CPU.Move("ecx", xFieldName + "___Contents"));
						aAssembler.Add(new CPU.Add("ecx", "0xC"));
						aAssembler.Add(new CPU.Move("dword [eax + 2]", "ecx"));
						break;
					}
				case GluePlaceholderMethodTypeEnum.IDT_Register: {
						FieldDefinition xFieldDef = GetGlueField(GlueFieldTypeEnum.IDT_Pointer);
						string xPointerFieldName;
						Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
						MethodDefinition xTheMethod = GetGlueMethod(GlueMethodTypeEnum.IDT_SetHandler);
						Engine.QueueMethod(xTheMethod);
						mIDTSetHandlerMethodName = new Label(xTheMethod).Name;
						aAssembler.Add(new CPU.Call("___________REGISTER___ISRS_____"));
						aAssembler.Add(new CPU.Move("eax", xPointerFieldName));
						aAssembler.Add(new Literal("XCHG BX, BX "));
						aAssembler.Add(new CPUNative.Lidt("eax"));
						GetGlueMethod(GlueMethodTypeEnum.IDT_InterruptHandler);
						break;
					}
				case GluePlaceholderMethodTypeEnum.IO_ReadByte: {
						aAssembler.Add(new CPU.Xor("eax", "eax"));
						IL.X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
						aAssembler.Add(new CPU.Pop("edx"));
						aAssembler.Add(new CPU.Move("eax", "0"));
						aAssembler.Add(new CPUNative.In("al", "dx"));
						aAssembler.Add(new CPU.Push("eax"));
						break;
					}
				case GluePlaceholderMethodTypeEnum.IO_WriteByte: {
						Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
						aAssembler.Add(new CPU.Pop("edx"));
						Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[1].VirtualAddresses, aMethodInfo.Arguments[1].Size);
						aAssembler.Add(new CPU.Pop("eax"));
						aAssembler.Add(new CPUNative.Out("dx", "al"));
						break;
					}
				case GluePlaceholderMethodTypeEnum.IDT_EnableInterrupts: {
						aAssembler.Add(new Literal("xchg bx, bx"));
						aAssembler.Add(new CPUNative.Sti());
						break;
					}
				default:
					throw new NotImplementedException("GluePlaceholderMethod '" + aMethodType.ToString() + "' not implemented!");
			}
		}

		private string mIDTSetHandlerMethodName;

		private static void DoAssemble_String_GetByteFromChar(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
		}

		public override void PostProcess(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			base.PostProcess(aAssembler);
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			MethodDefinition xTheMethod = GetGlueMethod(GlueMethodTypeEnum.IDT_SetHandler);
			Engine.QueueMethod(xTheMethod);
			mIDTSetHandlerMethodName = new Label(xTheMethod).Name;
			Console.WriteLine("IDTEntry size = {0}", Engine.GetFieldStorageSize(Engine.GetTypeDefinition(typeof(Cosmos.Kernel.Boot.Glue.IDTEntryStruct).Assembly.GetName().Name, typeof(Cosmos.Kernel.Boot.Glue.IDTEntryStruct).FullName)));
			X86.X86MethodHeaderOp.AssembleHeader(aAssembler, "___________REGISTER___ISRS_____", new int[0]);
			string xInterruptHandlerLabel = new Label(GetGlueMethod(GlueMethodTypeEnum.IDT_InterruptHandler)).Name;
			int[] xInterruptsWithParam = new int[] { 8, 10, 11, 12, 13, 14 };
			for (int i = 0; i < 256; i++) {
				aAssembler.Add(new CPU.Pushd("0x" + i.ToString("X")));
				aAssembler.Add(new CPU.Pushd("____INTERRUPT_HANDLER___" + i));
				aAssembler.Add(new CPU.Pushd("0x08"));
				aAssembler.Add(new CPU.Pushd("0x8E"));
				aAssembler.Add(new CPU.Call(mIDTSetHandlerMethodName));
			}
			X86.X86MethodFooterOp.AssembleFooter(0, aAssembler, new int[0], 0);
			for (int j = 0; j < 256; j++) {
				aAssembler.Add(new Label("____INTERRUPT_HANDLER___" + j));
				aAssembler.Add(new CPUNative.Cli());
				aAssembler.Add(new Literal("xchg bx, bx"));
				aAssembler.Add(new CPUNative.Pushad());
				aAssembler.Add(new CPU.Pushd(j.ToString()));
				if (!xInterruptsWithParam.Contains(j)/* && !(j >= 0x20 && j <= 0x2F)*/) {
					aAssembler.Add(new CPU.Pushd("0"));
				}
				aAssembler.Add(new CPU.Call(xInterruptHandlerLabel));
				aAssembler.Add(new CPUNative.Popad());
				aAssembler.Add(new Literal("xchg bx, bx"));
				aAssembler.Add(new CPUNative.Sti());
				aAssembler.Add(new CPUNative.IRet());
			}
		}
	}
}