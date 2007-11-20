using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel.Boot.Glue;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.IL.X86.Native.CustomImplementations.System;
using Indy.IL2CPU.IL.X86.Native.CustomImplementations.System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86.Native;

namespace Indy.IL2CPU.IL.X86.Native {
	public class NativeOpCodeMap: X86.X86OpCodeMap {
		internal static NativeOpCodeMap Instance;

		public override void Initialize(Indy.IL2CPU.Assembler.Assembler aAssembler, IEnumerable<AssemblyDefinition> aProjectAssemblies, Func<TypeReference, TypeDefinition> aTypeResolver, Func<string, AssemblyDefinition> aAssemblyResolver) {
			base.Initialize(aAssembler, aProjectAssemblies, aTypeResolver, aAssemblyResolver);
			base.mMap[Code.Call] = typeof(Call);
		}

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
						IEnumerable<CustomAttribute> xAttribs = (from item in xMethod.CustomAttributes.Cast<CustomAttribute>()
																 where item.Constructor.DeclaringType.FullName == typeof(GlueMethodAttribute).FullName
																 select item);
						if (xAttribs == null || xAttribs.Count() == 0) {
							continue;
						}
						foreach (CustomAttribute xAttrib in xAttribs) {
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
							throw new Exception("GlueField of type '" + ((GlueFieldTypeEnum)xAttrib.Properties["FieldType"]).ToString() + "' already found!");
						}
						mGlueFields.Add((GlueFieldTypeEnum)xAttrib.Properties["FieldType"], xFieldDefinition);
					}
				}
			}
		}

		protected override IList<AssemblyDefinition> GetPlugAssemblies() {
			IList<AssemblyDefinition> xResult =  base.GetPlugAssemblies();
			xResult.Add(AssemblyFactory.GetAssembly(typeof (NativeOpCodeMap).Assembly.Location));
			return xResult;
		}

		public override MethodReference GetCustomMethodImplementation_Old(string aOrigMethodName, bool aInMetalMode) {
			switch (aOrigMethodName) {
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
					return base.GetCustomMethodImplementation_Old(aOrigMethodName, aInMetalMode);
			}
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
						CheckGluePlaceholderMethod();
						GluePlaceholderMethodTypeEnum? xMethodType = null;
						return (from item in mGluePlaceholderMethods.Keys
								where Label.GenerateLabelName(mGluePlaceholderMethods[item]) == aMethodInfo.LabelName
								select item).Count() > 0;
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
						CheckGluePlaceholderMethod();
						GluePlaceholderMethodTypeEnum? xMethodType = null;

						foreach (GluePlaceholderMethodTypeEnum xTheMethodType in mGluePlaceholderMethods.Keys) {
							if (Label.GenerateLabelName(mGluePlaceholderMethods[xTheMethodType]) == aMethodInfo.LabelName) {
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
			base.DoCustomAssembleImplementation(aInMetalMode, aAssembler, aMethodInfo);
		}

		private void AssembleGluePlaceholderMethod(GluePlaceholderMethodTypeEnum aMethodType, Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			switch (aMethodType) {
				case GluePlaceholderMethodTypeEnum.GDT_LoadArray: {
						FieldDefinition xFieldDef = GetGlueField(GlueFieldTypeEnum.GDT_Array);
						string xFieldName = Assembler.DataMember.GetStaticFieldName(xFieldDef);
						string xFieldData = "0,0,0,0,";
						xFieldData += BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray).Aggregate("", (b, r) => r + b + ",");
						xFieldData += "3,0,0,0,";
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
						new CPU.Move("eax", xPointerFieldName);
						new CPU.Move("word [eax]", "0x" + ((8 * 3) - 1).ToString("X"));
						new CPU.Move("ecx", "[" + xFieldName + "]");
						new CPU.Add("ecx", "0xC");
						new CPU.Move("dword [eax + 2]", "ecx");
						break;
					}
				case GluePlaceholderMethodTypeEnum.GDT_Register: {
						FieldDefinition xFieldDef = GetGlueField(GlueFieldTypeEnum.GDT_Pointer);
						string xPointerFieldName;
						Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
						new CPU.Move("eax", xPointerFieldName);
						new CPUNative.Lgdt("[eax]");
						new CPU.Move("eax", "0x10");
						new CPU.Move("ds", "ax");
						new CPU.Move("es", "ax");
						new CPU.Move("fs", "ax");
						new CPU.Move("gs", "ax");
						new CPU.Move("ss", "ax");
						new CPU.JumpAlways("0x8:flush____gdt______table__part1");
						new Label("flush____gdt______table__part1");
						break;
					}
				case GluePlaceholderMethodTypeEnum.IDT_LoadArray: {
						MethodDefinition xTheMethod = GetGlueMethod(GlueMethodTypeEnum.IDT_SetHandler);
						Engine.QueueMethod(xTheMethod);
						mIDTSetHandlerMethodName = Label.GenerateLabelName(xTheMethod);
						FieldDefinition xFieldDef = GetGlueField(GlueFieldTypeEnum.IDT_Array);
						string xFieldName = Assembler.DataMember.GetStaticFieldName(xFieldDef);
						string xFieldData = "0,0,0,0,";
						xFieldData += BitConverter.GetBytes((uint)InstanceTypeEnum.StaticEmbeddedArray).Aggregate("", (b, r) => r + b + ",");
						xFieldData += "1,0,0,0";
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
						new CPU.Move("eax", xPointerFieldName);
						new CPU.Move("word [eax]", "0x" + ((8 * 256) - 1).ToString("X"));
						new CPU.Move("ecx", xFieldName + "___Contents");
						new CPU.Add("ecx", "0xC");
						new CPU.Move("dword [eax + 2]", "ecx");
						break;
					}
				case GluePlaceholderMethodTypeEnum.IDT_Register: {
						FieldDefinition xFieldDef = GetGlueField(GlueFieldTypeEnum.IDT_Pointer);
						string xPointerFieldName;
						Engine.QueueStaticField(xFieldDef, out xPointerFieldName);
						MethodDefinition xTheMethod = GetGlueMethod(GlueMethodTypeEnum.IDT_SetHandler);
						Engine.QueueMethod(xTheMethod);
						mIDTSetHandlerMethodName = Label.GenerateLabelName(xTheMethod);
						new CPU.Call("___________REGISTER___ISRS_____");
						new CPU.Move("eax", xPointerFieldName);
						new CPUNative.Lidt("eax");
						GetGlueMethod(GlueMethodTypeEnum.IDT_InterruptHandler);
						break;
					}
				case GluePlaceholderMethodTypeEnum.IO_ReadByte: {
						new CPU.Xor("eax", "eax");
						IL.X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
						new CPU.Pop("edx");
						new CPU.Move("eax", "0");
						new CPUNative.In("al", "dx");
						new CPU.Push("eax");
						break;
					}
				case GluePlaceholderMethodTypeEnum.IO_WriteByte: {
						Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
						new CPU.Pop("edx");
						Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[1]);
						new CPU.Pop("eax");
						new CPUNative.Out("dx", "al");
						break;
					}
				case GluePlaceholderMethodTypeEnum.IDT_EnableInterrupts: {
						new CPUNative.Sti();
						break;
					}
				case GluePlaceholderMethodTypeEnum.GetKernelResource: {
						new CPUNative.Break();
						break;
					}
				default:
					throw new NotImplementedException("GluePlaceholderMethod '" + aMethodType.ToString() + "' not implemented!");
			}
		}

		private string mIDTSetHandlerMethodName;

		private static void DoAssemble_String_GetByteFromChar(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			X86.Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
		}

		public override void PostProcess(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			base.PostProcess(aAssembler);
			TypeDefinition xRuntimeEngineTypeDef = Engine.GetTypeDefinition(typeof(RuntimeEngineImpl).Assembly.GetName().Name, typeof(RuntimeEngineImpl).FullName);
			MethodDefinition xTheMethod = GetGlueMethod(GlueMethodTypeEnum.IDT_SetHandler);
			Engine.QueueMethod(xTheMethod);
			mIDTSetHandlerMethodName = Label.GenerateLabelName(xTheMethod);
			Console.WriteLine("IDTEntry size = {0}", Engine.GetFieldStorageSize(Engine.GetTypeDefinition(typeof(Cosmos.Kernel.Boot.Glue.IDTEntryStruct).Assembly.GetName().Name, typeof(Cosmos.Kernel.Boot.Glue.IDTEntryStruct).FullName)));
			X86.X86MethodHeaderOp.AssembleHeader(aAssembler, "___________REGISTER___ISRS_____", new int[0], new MethodInformation.Argument[0]);
			string xInterruptHandlerLabel = Label.GenerateLabelName(GetGlueMethod(GlueMethodTypeEnum.IDT_InterruptHandler));
			int[] xInterruptsWithParam = new int[] { 8, 10, 11, 12, 13, 14 };
			for (int i = 0; i < 256; i++) {
				new CPU.Pushd("0x" + i.ToString("X"));
				new CPU.Pushd("____INTERRUPT_HANDLER___" + i);
				new CPU.Pushd("0x08");
				new CPU.Pushd("0x8E");
				new CPU.Call(mIDTSetHandlerMethodName);
			}
			X86.X86MethodFooterOp.AssembleFooter(0, aAssembler, new MethodInformation.Variable[0], new MethodInformation.Argument[0], 0);
			for (int j = 0; j < 256; j++) {
				new Label("____INTERRUPT_HANDLER___" + j);
				new CPUNative.Cli();
				new CPUNative.Break();
				new CPUNative.Pushad();
				new CPU.Pushd(j.ToString());
				if (!xInterruptsWithParam.Contains(j)/* && !(j >= 0x20 && j <= 0x2F)*/) {
					new CPU.Pushd("0");
				}
				new CPU.Call(xInterruptHandlerLabel);
				new CPUNative.Popad();
				new CPUNative.Break();
				new CPUNative.Sti();
				new CPUNative.IRet();
			}
			IEnumerable<AssemblyDefinition> xAssemblies = Engine.GetAllAssemblies();
			SortedList<int, byte[]> xResources = new SortedList<int, byte[]>();
			foreach (AssemblyDefinition xAssembly in xAssemblies) {
				foreach (CustomAttribute xAttrib in xAssembly.CustomAttributes) {
					if (!xAttrib.Resolved) {
						xAttrib.Resolve();
					}
				}
				IEnumerable<KeyValuePair<int, string>> xWantedResources = (from item in xAssembly.CustomAttributes.Cast<CustomAttribute>()
																		   where item.Constructor.DeclaringType.FullName == typeof(KernelResourceAttribute).FullName
																		   select new KeyValuePair<int, string>((int)item.ConstructorParameters[1],
																			   (string)item.ConstructorParameters[0]));
				foreach (KeyValuePair<int, string> xWantedResource in xWantedResources) {
					foreach (ModuleDefinition xModule in xAssembly.Modules) {
						foreach (Resource xResource in xModule.Resources) {
							if (xResource.Name == xWantedResource.Value) {
								EmbeddedResource xEmbedded = xResource as EmbeddedResource;
								if (xEmbedded == null) {
									throw new Exception("Resource found but was not an embedded resource (Resource Name = '" + xWantedResource.Value + "')");
								}
								xResources.Add(xWantedResource.Key, xEmbedded.Data);
							}
						}
					}
				}
			}
			StringBuilder xValue = new StringBuilder();
			DataMember xDataMember;
			for (int i = 0; i < xResources.Count; i++) {
				xValue.Remove(0, xValue.Length);
				xValue.Append("0,0,0,0,2,0,0,0,");
				xValue.Append(BitConverter.GetBytes(xResources.Values[i].Length).Aggregate("", (r, v) => r + v + ","));
				xValue.Append(xResources.Values[i].Aggregate("", (r, v) => r + v + ","));
				xDataMember = new DataMember("embedded_resource_" + xResources.Keys[i] + "_contents", "db", xValue.ToString().TrimEnd(','));
				aAssembler.DataMembers.Add(xDataMember);
			}
			MethodDefinition xGetResourceMethod = GetGluePlaceholderMethod(GluePlaceholderMethodTypeEnum.GetKernelResource);
			MethodInformation xGetResourceMethodInfo = Engine.GetMethodInfo(xGetResourceMethod, xGetResourceMethod, xGetResourceMethod.Name, null);
			string xGetResourceLabelName = Label.GenerateLabelName(xGetResourceMethod);
			for (int i = 0; i < aAssembler.Instructions.Count; i++) {
				if ((aAssembler.Instructions[i] is Label) && ((Label)aAssembler.Instructions[i]).Name == xGetResourceLabelName) {
					aAssembler.Instructions.RemoveRange(i, 10);
					break;
				}
			}
			NativeMethodHeaderOp.AssembleHeader(aAssembler, xGetResourceLabelName, new int[0], new MethodInformation.Argument[0]);
			Op.Ldarg(aAssembler, xGetResourceMethodInfo.Arguments[0]);
			new CPU.Pop("eax");
			foreach (int xId in xResources.Keys) {
				new CPU.Move("ecx", "0x" + xId.ToString("X"));
				new CPU.Compare("eax", "ecx");
				new CPU.JumpIfNotEquals(".__after__" + xId);
				new CPU.Push("embedded_resource_" + xId + "_contents");
				new CPU.JumpAlways(".END__OF__METHOD");
				new Label(".__after__" + xId);
			}
			NativeMethodFooterOp.AssembleFooter(4, aAssembler, new MethodInformation.Variable[0], new MethodInformation.Argument[0], 4);
		}
	}
}