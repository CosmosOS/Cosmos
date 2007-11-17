using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class X86OpCodeMap: OpCodeMap {
		protected override Type GetMethodHeaderOp() {
			return typeof(X86MethodHeaderOp);
		}

		protected override Type GetMethodFooterOp() {
			return typeof(X86MethodFooterOp);
		}

		protected override Type GetInitVmtImplementationOp() {
			return typeof(X86InitVmtImplementationOp);
		}

		protected override Assembly ImplementationAssembly {
			get {
				return typeof(X86OpCodeMap).Assembly;
			}
		}

		protected override Type GetMainEntryPointOp() {
			return typeof(X86MainEntryPointOp);
		}

		protected override Type GetPInvokeMethodBodyOp() {
			return typeof(X86PInvokeMethodBodyOp);
		}

		protected override Type GetCustomMethodImplementationProxyOp() {
			return typeof(X86CustomMethodImplementationProxyOp);
		}

		public override Mono.Cecil.MethodReference GetCustomMethodImplementation(string aOrigMethodName, bool aInMetalMode) {
			switch (aOrigMethodName) {
				case "System_Int32___System_String_get_Length____": {
						return CustomImplementations.System.StringImplRefs.get_LengthRef;
					}
				case "System_UInt32____Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_String___": {
						return CustomImplementation.System.StringImplRefs.GetStorage_ImplRef;
					}
				case "System_IntPtr___System_Delegate_GetInvokeMethod____": {
						return CustomImplementations.System.EventHandlerImplRefs.GetInvokeMethodRef;
					}
				case "System_Char___System_String_get_Chars___System_Int32___": {
						if (aInMetalMode) {
							return CustomImplementations.System.StringImplRefs.get_Chars_MetalRef;
						} else {
							return CustomImplementations.System.StringImplRefs.get_Chars_NormalRef;
						}
					}
				default:
					return base.GetCustomMethodImplementation(aOrigMethodName, aInMetalMode);
			}
		}

		public override bool HasCustomAssembleImplementation(MethodInformation aMethodInfo, bool aInMetalMode) {
			switch (aMethodInfo.LabelName) {
				case "System_Object___System_Threading_Interlocked_CompareExchange___System_Object___System_Object__System_Object___": {
						return true;
					}
				case "System_Int32___System_Threading_Interlocked_CompareExchange___System_Int32___System_Int32__System_Int32___": {
						return true;
					}
				case "System_String___System_String_FastAllocateString___System_Int32___": {
						return true;
					}
				case "System_Void___System_EventHandler_Invoke___System_Object__System_EventArgs___": {
						return true;
					}
				case "System_IntPtr___System_Delegate_GetInvokeMethod____": {
						return true;
					}
				case "System_IntPtr___System_Delegate_GetMulticastInvoke____": {
						return true;
					}
				case "System_MulticastDelegate___System_Delegate_InternalAllocLike___System_Delegate___": {
						return true;
					}
				default: {
						// we need special treatment for delegate constructors, which have an Object,IntPtr param list
						if (ObjectUtilities.IsDelegate(aMethodInfo.MethodDefinition.DeclaringType)) {
							if (aMethodInfo.LabelName.EndsWith("__ctor___System_Object__System_IntPtr___")) {
								return true;
							}
							if (aMethodInfo.MethodDefinition.Name == "Invoke") {
								return true;
							}
						}

						return base.HasCustomAssembleImplementation(aMethodInfo, aInMetalMode);
					}
			}
		}

		public override void DoCustomAssembleImplementation(bool aInMetalMode, Indy.IL2CPU.Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			switch (aMethodInfo.LabelName) {
				case "System_Object___System_Threading_Interlocked_CompareExchange___System_Object___System_Object__System_Object___": {
						Assemble_System_Threading_Interlocked_CompareExchange__Object(aAssembler, aMethodInfo);
						break;
					}
				case "System_Int32___System_Threading_Interlocked_CompareExchange___System_Int32___System_Int32__System_Int32___": {
						Assemble_System_Threading_Interlocked_CompareExchange__Object(aAssembler, aMethodInfo);
						break;
					}
				case "System_IntPtr___System_Delegate_GetMulticastInvoke____": {
						Engine.QueueMethodRef(CustomImplementations.System.EventHandlerImplRefs.MulticastInvokeRef);
						new CPUx86.Push(CPU.Label.GenerateLabelName(CustomImplementations.System.EventHandlerImplRefs.MulticastInvokeRef));
						break;
					}
				case "System_MulticastDelegate___System_Delegate_InternalAllocLike___System_Delegate___": {
						break;
					}
				default:
					if (ObjectUtilities.IsDelegate(aMethodInfo.MethodDefinition.DeclaringType)) {
						if (aMethodInfo.LabelName.EndsWith("__ctor___System_Object__System_IntPtr___")) {
							for (int i = 0; i < aMethodInfo.Arguments.Length; i++) {
								Op.Ldarg(aAssembler, aMethodInfo.Arguments[i].VirtualAddresses, aMethodInfo.Arguments[i].Size);
							}
							new Call(CustomImplementations.System.EventHandlerImplRefs.CtorRef) {
								Assembler = aAssembler
							}.Assemble();
							break;
						}
						if (aMethodInfo.MethodDefinition.Name == "Invoke") {
							// param 0 is instance of eventhandler
							// param 1 is sender
							// param 2 is eventargs
							Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
							new CPUx86.Push("0x" + (ObjectImpl.FieldDataOffset + 4).ToString("X"));
							aAssembler.StackSizes.Push(4);
							Ldarg.Add(aAssembler);
							new CPUx86.Pop("eax");
							new CPUx86.Pushd("[eax]");
							for (int i = 1; i < aMethodInfo.Arguments.Length; i++) {
								Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[i].VirtualAddresses, aMethodInfo.Arguments[i].Size);
							}
							Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
							new CPUx86.Push("0x" + ObjectImpl.FieldDataOffset.ToString("X"));
							aAssembler.StackSizes.Push(4);
							Ldarg.Add(aAssembler);
							new CPUx86.Pop("eax");
							new CPUx86.Pushd("[eax]");
							new CPUx86.Pop("eax");
							new CPUx86.Call("eax");
							new CPUx86.Pop("eax");
							break;
						}
					}
					base.DoCustomAssembleImplementation(aInMetalMode, aAssembler, aMethodInfo);
					break;
			}
		}

		private static void Assemble_System_Threading_Interlocked_CompareExchange__Object(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			//arguments:
			//   0: location
			//   1: value
			//   2: comparand
			Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[2].VirtualAddresses, aMethodInfo.Arguments[2].Size);
			new CPUx86.Pop("eax");
			Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[1].VirtualAddresses, aMethodInfo.Arguments[1].Size);
			new CPUx86.Pop("edx");
			Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
			new CPUx86.Pop("ecx");
			new CPUx86.Pushd("[ecx]");
			new CPUx86.Pop("ecx");
			new CPUx86.CmpXchg("ecx", "edx");
			Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0].VirtualAddresses, aMethodInfo.Arguments[0].Size);
			new CPUx86.Pop("eax");
			new CPUx86.Move("[eax]", "ecx");
			new CPUx86.Pushd("eax");
		}
	}
}
