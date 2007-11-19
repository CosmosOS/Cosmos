using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
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

		protected override IList<AssemblyDefinition> GetPlugAssemblies() {
			IList<AssemblyDefinition> xResult = base.GetPlugAssemblies();
			xResult.Add(AssemblyFactory.GetAssembly(typeof(X86OpCodeMap).Assembly.Location));
			return xResult;
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
								Op.Ldarg(aAssembler, aMethodInfo.Arguments[i]);
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
							Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
							new CPUx86.Push("0x" + (ObjectImpl.FieldDataOffset + 4).ToString("X"));
							aAssembler.StackSizes.Push(4);
							Ldarg.Add(aAssembler);
							new CPUx86.Pop(CPUx86.Registers.EAX);
							new CPUx86.Pushd(CPUx86.Registers.AtEAX);
							for (int i = 1; i < aMethodInfo.Arguments.Length; i++) {
								Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[i]);
							}
							Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
							new CPUx86.Push("0x" + ObjectImpl.FieldDataOffset.ToString("X"));
							aAssembler.StackSizes.Push(4);
							Ldarg.Add(aAssembler);
							new CPUx86.Pop(CPUx86.Registers.EAX);
							new CPUx86.Pushd(CPUx86.Registers.AtEAX);
							new CPUx86.Pop(CPUx86.Registers.EAX);
							new CPUx86.Call(CPUx86.Registers.EAX);
							new CPUx86.Pop(CPUx86.Registers.EAX);
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
			Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[2]);
			new CPUx86.Pop(CPUx86.Registers.EAX);
			Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[1]);
			new CPUx86.Pop(CPUx86.Registers.EDX);
			Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
			new CPUx86.Pop(CPUx86.Registers.ECX);
			new CPUx86.Pushd(CPUx86.Registers.AtECX);
			new CPUx86.Pop(CPUx86.Registers.ECX);
			new CPUx86.CmpXchg(CPUx86.Registers.ECX, CPUx86.Registers.EDX);
			Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Move(CPUx86.Registers.EAX, CPUx86.Registers.ECX);
			new CPUx86.Pushd(CPUx86.Registers.EAX);
		}
	}
}
