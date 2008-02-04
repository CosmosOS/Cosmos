using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPU = Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Cgt)]
	public class Cgt: Op {
		private readonly string NextInstructionLabel;
		private readonly string CurInstructionLabel;
		public Cgt(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			NextInstructionLabel = GetInstructionLabel(aReader.Position);
			CurInstructionLabel = GetInstructionLabel(aReader);
		}
		public override void DoAssemble() {
			var xStackItem= Assembler.StackContents.Pop();
			if (xStackItem.IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			if (xStackItem.Size > 8) {
				throw new Exception("StackSizes>8 not supported");
			}
			Assembler.StackContents.Push(new StackContent(4, typeof(bool)));
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			new CPUx86.Pop(CPUx86.Registers.EAX);
			if (xStackItem.Size > 4) {
				new CPUx86.Add("esp", "4");
			}
			new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
			new CPUx86.JumpIfGreater(LabelTrue);
			new CPUx86.JumpAlways(LabelFalse);
			new CPU.Label(LabelTrue);
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
			if (xStackItem.Size > 4) {
				new CPUx86.Add("esp", "4");
			}
			new CPUx86.Push("01h");
			new CPUx86.JumpAlways(NextInstructionLabel);
			new CPU.Label(LabelFalse);
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
			if (xStackItem.Size > 4) {
				new CPUx86.Add("esp", "4");
			}
			new CPUx86.Push("00h");
			new CPUx86.JumpAlways(NextInstructionLabel);
		}
	}
}