using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodFooterOp: MethodFooterOp {
		public readonly int TotalArgsSize = 0;
		public readonly int ReturnSize = 0;
		public readonly MethodInformation.Variable[] Locals;
		public readonly MethodInformation.Argument[] Args;

		public X86MethodFooterOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			if (aMethodInfo != null) {
				//			if (aMethodInfo.Locals.Length > 0) {
				//				TotalLocalsSize += aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Offset + aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Size;
				//			}
				Locals = aMethodInfo.Locals.ToArray();
				Args = aMethodInfo.Arguments.ToArray();
				ReturnSize = aMethodInfo.ReturnSize;
			}
		}

		public override void DoAssemble() {
			AssembleFooter(ReturnSize, Assembler, Locals, Args, (from item in Args
																 select item.Size).Sum());
		}

		public static void AssembleFooter(int aReturnSize, Assembler.Assembler aAssembler, MethodInformation.Variable[] aLocals, MethodInformation.Argument[] aArgs, int aTotalArgsSize) {
			new Label(".END__OF__METHOD");
			if (!aAssembler.InMetalMode) {
				Engine.QueueMethodRef(GCImplementationRefs.DecRefCountRef);
				foreach (MethodInformation.Variable xLocal in aLocals) {
					if (xLocal.IsReferenceType && xLocal.VariableType.FullName != "System.String") {
					//	System.Diagnostics.Debugger.Break();
						TypeSpecification xTypeSpec = xLocal.VariableType as TypeSpecification;
						if (xTypeSpec != null) {
							TypeDefinition xElementDef = Engine.GetDefinitionFromTypeReference(xTypeSpec.ElementType);
							if ((!xElementDef.IsValueType) && xElementDef.FullName != "System.String") {
								Op.Ldloc(aAssembler, xLocal, false);
								new CPUx86.Push("8");
								Op.Add(aAssembler);
								new CPUx86.Pop(CPUx86.Registers.EDX); // total item count	address
								new CPUx86.Move(CPUx86.Registers.EBX, CPUx86.Registers.AtEDX);
								new CPUx86.Add(CPUx86.Registers.EDX, "4");
								new CPUx86.Move(CPUx86.Registers.ECX, "0"); // counter
								new Label(".GC_LOCAL_CLEANUP_ENTRY_VAR_" + xLocal.Offset);
								new CPUx86.Compare(CPUx86.Registers.EBX, CPUx86.Registers.ECX);
								new CPUx86.JumpIfEquals(".GC_LOCAL_CLEANUP_ENTRY_VAR_" + xLocal.Offset + "_END");
								new CPUx86.Push(CPUx86.Registers.EDX);
								new CPUx86.Push(CPUx86.Registers.ECX);
								new CPUx86.Push(CPUx86.Registers.EBX);
								new CPUx86.Push("dword", CPUx86.Registers.AtEDX);
								new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
								new CPUx86.Pop(CPUx86.Registers.EBX);
								new CPUx86.Pop(CPUx86.Registers.ECX);
								new CPUx86.Pop(CPUx86.Registers.EDX);
								new CPUx86.Add(CPUx86.Registers.EDX, "4");
								new CPUx86.Add(CPUx86.Registers.ECX, "1");
								new CPUx86.JumpAlways(".GC_LOCAL_CLEANUP_ENTRY_VAR_" + xLocal.Offset);
								new Label(".GC_LOCAL_CLEANUP_ENTRY_VAR_" + xLocal.Offset + "_END");
								// label
								// item pushen
								// ecx ophogen
								// ecx en eax vergelijken
								// als ecx kleiner is, naar label
							}
						}
						Op.Ldloc(aAssembler, xLocal, false);
						new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
					}
				}
				foreach (MethodInformation.Argument xArg in aArgs) {
					if (xArg.IsReferenceType && xArg.ArgumentType.FullName != "System.String") {
						Op.Ldarg(aAssembler, xArg, false);
						new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
					}
				}
			}
			if (aReturnSize > 0) {
				if (aReturnSize > 4) {
					throw new Exception("ReturnValue sizes larger than 4 not supported yet");
				} else {
					new Assembler.X86.Pop(CPUx86.Registers.EAX);
				}
			}
			for (int j = aLocals.Length - 1; j >= 0; j--) {
				int xLocalSize = aLocals[j].Size;
				new CPUx86.Add(CPUx86.Registers.ESP, "0x" + xLocalSize.ToString("X"));
			}
			new CPUx86.Popd(CPUx86.Registers.EBP);
			new CPUx86.Ret(aTotalArgsSize == 0 ? "" : aTotalArgsSize.ToString());
		}
	}
}