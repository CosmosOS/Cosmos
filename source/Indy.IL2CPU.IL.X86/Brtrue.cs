using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Brtrue)]
	public class Brtrue: Op {
		public readonly string TargetLabel;
		public Brtrue(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		}

		public override void DoAssemble() {
			int xSize = Assembler.StackContents.Pop().Size;
			if (xSize > 8) {
				throw new Exception("StackSize>8 not supported");
			}
			if (xSize > 4)
			{
				new CPUx86.Popd("eax");
				new CPUx86.Popd("ebx");
				new CPUx86.Xor("eax", "eax");
				new CPUx86.JumpIfNotZero(TargetLabel);
				new CPUx86.Xor("ebx", "ebx");
				new CPUx86.JumpIfNotZero(TargetLabel);
			} else
			{
				new CPUx86.Popd(CPUx86.Registers.EAX);
				new CPUx86.Compare(CPUx86.Registers.EAX, "0");
				new CPUx86.JumpIfNotEquals(TargetLabel);
			}
		}
	}
}