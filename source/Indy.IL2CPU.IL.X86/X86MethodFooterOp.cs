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
			new Label(EndOfMethodLabelName);
			if (!aAssembler.InMetalMode) {
				Engine.QueueMethodRef(GCImplementationRefs.DecRefCountRef);
				foreach (MethodInformation.Variable xLocal in aLocals) {
					if (xLocal.IsReferenceType) {
						Op.Ldloc(aAssembler, xLocal, false);
						new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
					}
				}
				foreach (MethodInformation.Argument xArg in aArgs) {
					if (xArg.IsReferenceType) {
						Op.Ldarg(aAssembler, xArg, false);
						new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
					}
				}
			}
			if (aReturnSize > 0) {
				if (aReturnSize > 8) {
					throw new Exception("ReturnValue sizes larger than 8 not supported yet");
				} else {
					if (aReturnSize <= 4) {
						new Assembler.X86.Pop(CPUx86.Registers.EAX);
					} else {
						new Assembler.X86.Pop(CPUx86.Registers.EAX);
						new Assembler.X86.Pop(CPUx86.Registers.EBX);
					}
				}
			}
			for (int j = aLocals.Length - 1; j >= 0; j--) {
				int xLocalSize = aLocals[j].Size;
				new CPUx86.Add(CPUx86.Registers.ESP, "0x" + xLocalSize.ToString("X"));
			}
			//new CPUx86.Add(CPUx86.Registers.ESP, "0x4");
			new CPUx86.Popd(CPUx86.Registers.EBP);
			new CPUx86.Ret(aTotalArgsSize == 0 ? "" : aTotalArgsSize.ToString());
		}
	}
}