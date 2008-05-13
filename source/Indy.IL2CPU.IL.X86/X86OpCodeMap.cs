using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.IL.X86.CustomImplementations.System;

namespace Indy.IL2CPU.IL.X86 {
	public  class X86OpCodeMap: OpCodeMap {
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

		protected override Type GetCustomMethodImplementationOp() {
			return null;
		}

		public override IList<Assembly> GetPlugAssemblies() {
			IList<Assembly> xResult = base.GetPlugAssemblies();
			xResult.Add(typeof(X86OpCodeMap).Assembly);
			return xResult;
		}

		public override bool HasCustomAssembleImplementation(MethodInformation aMethodInfo, bool aInMetalMode) {
			switch (aMethodInfo.LabelName) {
                case "System_Object__System_Threading_Interlocked_CompareExchange_System_Object___System_Object__System_Object_":
                    {
						return true;
					}
				case "System_Int32__System_Threading_Interlocked_CompareExchange_System_Int32___System_Int32__System_Int32_": {
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
                case "System_IntPtr__System_Delegate_GetMulticastInvoke__":
                    {
				        return true;
				    }
				case "System_MulticastDelegate___System_Delegate_InternalAllocLike___System_Delegate___": {
						return true;
					}
				default: {
						// we need special treatment for delegate constructors, which have an Object,IntPtr param list
						if (ObjectUtilities.IsDelegate(aMethodInfo.Method.DeclaringType)) {
							if (aMethodInfo.LabelName.EndsWith("__ctor_System_Object__System_IntPtr_")) {
								return true;
							}
							if (aMethodInfo.Method.Name == "Invoke") {
								return true;
							}
						}
						return base.HasCustomAssembleImplementation(aMethodInfo, aInMetalMode);
					}
			}
		}

        private static readonly MethodBase InvokeMulticastRef = typeof(MulticastDelegateImpl).GetMethod("InvokeMulticast", BindingFlags.Public | BindingFlags.Static);

		public override void DoCustomAssembleImplementation(bool aInMetalMode, Indy.IL2CPU.Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			switch (aMethodInfo.LabelName) {
                case "System_Object__System_Threading_Interlocked_CompareExchange_System_Object___System_Object__System_Object_":
                    {
						Assemble_System_Threading_Interlocked_CompareExchange__Object(aAssembler, aMethodInfo);
						break;
					}
				case "System_Int32__System_Threading_Interlocked_CompareExchange_System_Int32___System_Int32__System_Int32_": {
						Assemble_System_Threading_Interlocked_CompareExchange__Object(aAssembler, aMethodInfo);
						break;
					}
                case "System_IntPtr__System_Delegate_GetMulticastInvoke__":
                    {
                        //IL.X86.Op.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
                        
                        Engine.QueueMethod(InvokeMulticastRef);
                    new CPUx86.Push(Label.GenerateLabelName(InvokeMulticastRef));
				        break;
				    }
				case "System_MulticastDelegate___System_Delegate_InternalAllocLike___System_Delegate___": {
						break;
					}
				default:
					if (ObjectUtilities.IsDelegate(aMethodInfo.Method.DeclaringType)) {
						if (aMethodInfo.LabelName.EndsWith("__ctor_System_Object__System_IntPtr_")) {
							//for (int i = 0; i < aMethodInfo.Arguments.Length; i++) {
							//    Op.Ldarg(aAssembler, aMethodInfo.Arguments[i]);
							//}
							//new Call(CustomImplementations.System.EventHandlerImplRefs.CtorRef) {
							//    Assembler = aAssembler
							//}.Assemble();
							var xOp = new X86CustomMethodImplementationProxyOp(null, aMethodInfo);
							xOp.Assembler = aAssembler;
							xOp.ProxiedMethod = CustomImplementations.System.EventHandlerImplRefs.CtorRef;
							xOp.Assemble();
                            Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
                            //
                            var xInvokeMethod = aMethodInfo.TypeInfo.TypeDef.GetMethod("Invoke");
                            var xDelegateMethodInfo = Engine.GetMethodInfo(xInvokeMethod, xInvokeMethod, xInvokeMethod.Name, aMethodInfo.TypeInfo, aMethodInfo.DebugMode);
                            int xArgSize = (from item in xDelegateMethodInfo.Arguments
                                            select item.Size + (item.Size % 4 == 0
                                                                    ? 0
                                                                    : (4 - (item.Size % 4)))).Take(xDelegateMethodInfo.Arguments.Length).Sum();
                            new CPUx86.Push((xArgSize-4).ToString());
                            new CPU.Label(".SetArgSize");
                            aAssembler.StackContents.Push(new StackContent(4));
                            Stfld.Stfld(aAssembler, aMethodInfo.TypeInfo, aMethodInfo.TypeInfo.Fields["$$ArgSize$$"]);
							break;
						}
                        if (aMethodInfo.Method.Name == "Invoke") {
							// param 0 is instance of eventhandler
							// param 1 is sender
							// param 2 is eventargs
						    new Label(".LoadTargetObject");
							Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
                            //Ldarg.Ldfld(aAssembler, aMethodInfo.TypeInfo, "System.Object System.Delegate._target");
                            //new Label(".LoadMethodParams");
                            //for (int i = 1; i < aMethodInfo.Arguments.Length; i++) {
                            //    Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[i]);
                            //}
                            //new Label(".LoadMethodPointer");
                            //Ldarg.Ldarg(aAssembler, aMethodInfo.Arguments[0]);
                            //Ldarg.Ldfld(aAssembler, aMethodInfo.TypeInfo, "System.IntPtr System.Delegate._methodPtr");
                            //new CPUx86.Pop("eax");
                            //new CPUx86.Call(CPUx86.Registers.EAX);
                            //new CPUx86.Add("esp",
                            //               "4");
                            Engine.QueueMethod(InvokeMulticastRef);
                            new CPUx86.Call(Label.GenerateLabelName(InvokeMulticastRef));
                            var xGetInvocationListMethod = typeof(MulticastDelegate).GetMethod("GetInvocationList");
                            Engine.QueueMethod(xGetInvocationListMethod);
                            xGetInvocationListMethod = typeof(Delegate).GetMethod("GetInvocationList");
                            Engine.QueueMethod(xGetInvocationListMethod);

							//							new CPUx86.Pop(CPUx86.Registers.EAX);
							//new CPUx86.Move("esp", "ebp");
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

		public override void EmitOpDebugHeader(Indy.IL2CPU.Assembler.Assembler aAssembler, uint aOpId, string aOpLabel) {
			new CPUx86.Call("DebugPoint__");
		}
	}
}
