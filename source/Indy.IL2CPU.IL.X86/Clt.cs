using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPU = Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Clt)]
	public class Clt: Op {
		private readonly string NextInstructionLabel;
		private readonly string CurInstructionLabel;
		public Clt(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
		}
		public override void DoAssemble() {
			if (Assembler.StackContents.Peek().IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
			if (xSize > 8) {
				throw new Exception("StackSizes>8 not supported");
			}
			Assembler.StackContents.Push(new StackContent(1, typeof(bool)));
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			if (xSize > 4)
			{
				new CPUx86.Xor("esi", "esi");
				new CPUx86.Add("esi", "1");
				new CPUx86.Xor("edi", "edi");
				//esi = 1
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Pop(CPUx86.Registers.EDX);
				//value2: EDX:EAX
				new CPUx86.Pop("ebx");
				new CPUx86.Pop("ecx");
				//value1: ECX:EBX
				new CPUx86.Sub("ebx", "eax");
				new CPUx86.SubWithCarry("ecx", "edx");
				//result = value1 - value2
				new CPUx86.ConditionalMove(Condition.Less, "edi", "esi");
				new CPUx86.Push("edi");
			} else
			{
				new CPUx86.Pop(CPUx86.Registers.ECX);
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Pushd(CPUx86.Registers.ECX);
				new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
				new CPUx86.JumpIfLess(LabelTrue);
				new CPUx86.JumpAlways(LabelFalse);
				new CPU.Label(LabelTrue);
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
				new CPUx86.Push("01h");
				new CPUx86.JumpAlways(NextInstructionLabel);
				new CPU.Label(LabelFalse);
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
				new CPUx86.Push("00h");
				new CPUx86.JumpAlways(NextInstructionLabel);
			}
		}
	}
}