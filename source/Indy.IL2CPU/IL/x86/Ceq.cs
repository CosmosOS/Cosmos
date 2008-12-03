using System;
using System.Linq;

using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ceq)]
	public class Ceq: Op {
		private readonly string NextInstructionLabel;
		private readonly string CurInstructionLabel;
		public Ceq(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
		}
		private void Assemble4Byte() {
			Assembler.StackContents.Push(new StackContent(4, typeof(bool)));
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = LabelTrue };
            new CPUx86.Jump { DestinationLabel = LabelFalse };
			new CPU.Label(LabelTrue);
			new CPUx86.Add{DestinationReg = CPUx86.Registers.ESP, SourceValue=4};
            new CPUx86.Push { DestinationValue = 1 };
            new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
			new CPU.Label(LabelFalse);
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
			new CPUx86.Push{DestinationValue=0};
            new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
		}

		private void Assemble8Byte() {
			Assembler.StackContents.Push(new StackContent(4, typeof(bool)));
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = LabelFalse };

            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };

			//they are equal, eax == 0
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Jump { DestinationLabel = NextInstructionLabel };

			new CPU.Label(LabelFalse);
			//eax = 0
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
		}

		public override void DoAssemble() {
			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
			if (xSize > 8) {
				throw new Exception("StackSizes>8 not supported");
			}
			if (xSize <= 4) {
				Assemble4Byte();
				return;
			}
			if (xSize > 4) {
				Assemble8Byte();
				return;
			}
			throw new Exception("Case not handled!");
		}
	}
}