using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Blt)]
	public class Blt: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public readonly string NextInstructionLabel;
		public Blt(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
			NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
		}
		public override void DoAssemble() {
			if (Assembler.StackContents.Peek().IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			var right = Assembler.StackContents.Pop();
			var left = Assembler.StackContents.Pop();
			if (right.Size != left.Size)
				throw new NotImplementedException("mixed size operations are not implemented");

			int xSize = right.Size;

			if (xSize > 8)
				throw new NotImplementedException("StackSize>8 not supported");

			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";

			switch(xSize)
			{
			case 4:
				new CPUx86.Pop(CPUx86.Registers.ECX);
				//if (xSize > 4)
				//{
				//    throw new NotImplementedException("long comprasion is not implemented");
				//    new CPUx86.Add("esp", "4");
				//}
				new CPUx86.Pop(CPUx86.Registers.EAX);
				//if (xSize > 4)
				//{
				//    throw new NotImplementedException("long comprasion is not implemented");
				//    new CPUx86.Add("esp", "4");
				//}
				new CPUx86.Pushd(CPUx86.Registers.ECX);
				new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
				new CPUx86.JumpIfLess(LabelTrue);
				new CPUx86.JumpAlways(LabelFalse);
				new CPU.Label(LabelTrue);
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
				new CPUx86.JumpAlways(TargetLabel);
				new CPU.Label(LabelFalse);
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
				break;
			case 8:
				new CPUx86.Pop(CPUx86.Registers.EBX);				//ebx = lowright
				new CPUx86.Pop(CPUx86.Registers.EAX);				//eax = highright
				new CPUx86.Pop(CPUx86.Registers.ECX);				//ecx = lowleft
				new CPUx86.Pop(CPUx86.Registers.EDX);				//edx = high left

				new CPUx86.Compare(CPUx86.Registers.EDX, CPUx86.Registers.EAX);

				new CPUx86.JumpIfLess(LabelTrue);					//значение на глубине меньше
				new CPUx86.JumpIfGreater(NextInstructionLabel);		//значение на глубине больше

				if ((0xFFFFFFFF > 0xFFFFFFE) && (0xFFFF0000 < 0xFFFFFFFE))
				{
					new CPUx86.Compare(CPUx86.Registers.ECX, CPUx86.Registers.EBX);

					new CPUx86.JumpIfLess(LabelTrue);
					new CPUx86.JumpAlways(NextInstructionLabel);
				} else
					throw new NotImplementedException("long comprasion is not implemented");
																
				//значения старших 														

				new CPU.Label(LabelTrue);
				new CPUx86.JumpAlways(TargetLabel);

				//throw new NotImplementedException("long comprasion is not implemented");
				break;
			default:
				throw new NotSupportedException(string.Format("comprasion of {0} byte values", xSize));
			}
		}
	}
}