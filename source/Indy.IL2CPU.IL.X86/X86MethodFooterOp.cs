using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MethodFooterOp: MethodFooterOp {
		public readonly int TotalArgsSize = 0;
		public readonly int ReturnSize = 0;
		public readonly int[] Locals;
		public readonly int[] Args;

		public X86MethodFooterOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			if (aMethodInfo != null) {
				//			if (aMethodInfo.Locals.Length > 0) {
				//				TotalLocalsSize += aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Offset + aMethodInfo.Locals[aMethodInfo.Locals.Length - 1].Size;
				//			}
				Locals = new int[aMethodInfo.Locals.Length];
				for (int i = 0; i < aMethodInfo.Locals.Length; i++) {
					var xVar = aMethodInfo.Locals[i];
					Locals[i] = xVar.Size;
					if (xVar.Size % 4 != 0) {
						throw new Exception("Local Variable size is not a a multiple of 4");
					}
				}
				Args = new int[aMethodInfo.Arguments.Length];
				for (int i = 0; i < aMethodInfo.Arguments.Length; i++) {
					var xArg = aMethodInfo.Arguments[i];
					Args[i] = xArg.Size;
					if (xArg.Size % 4 != 0) {
						throw new Exception("Argument size is not a a multiple of 4");
					}
				}
				ReturnSize = aMethodInfo.ReturnSize;
			}
		}

		public override void DoAssemble() {
			// MtW: after trial and a huge amount of errors, this line doesn't seem to be needed
			//Assembler.Add(new CPU.Add("esp", TotalLocalsSize.ToString()));
			AssembleFooter(ReturnSize, Assembler, Locals, Args.Sum());
		}

		public static void AssembleFooter(int aReturnSize, Assembler.Assembler aAssembler, int[] aLocalsSizes, int aTotalArgsSize) {
			if (aReturnSize > 0) {
				if (aReturnSize > 4) {
					throw new Exception("ReturnValue sizes larger than 4 not supported yet");
				}
				aAssembler.Add(new Assembler.X86.Pop("eax"));
				aAssembler.StackSizes.Pop();
			}
			for (int j = (aLocalsSizes.Length - 1); j >= 0; j--) {
				int xLocalSize = aLocalsSizes[j];
				aAssembler.StackSizes.Pop();
				aAssembler.Add(new CPU.Add("esp", "0x" + xLocalSize.ToString("X")));
			}
			aAssembler.Add(new CPU.Pop("ebp"));
			aAssembler.Add(new CPU.Ret(aTotalArgsSize == 0 ? "" : aTotalArgsSize.ToString()));
		}
	}
}