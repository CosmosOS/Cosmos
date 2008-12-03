using System;
using System.IO;
using CPU = Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_Ovf_I)]
	public class Conv_Ovf_I: Op {
		private readonly string NextInstructionLabel;
		public Conv_Ovf_I(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
				NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
		}
		public override void DoAssemble() {
			var xSource = Assembler.StackContents.Pop();
			switch (xSource.Size)
			{
			case 1:
			case 2:
			case 4:
				{
					new CPUx86.Noop();
					break;
				}
			case 8:
				{
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.SignExtendAX { Size = 32 };
					//all bits of EDX == sign (EAX)
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
					//must be equal to EDX
                    new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EDX };
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = NextInstructionLabel };
					//equals
					new CPUx86.Interrupt{DestinationValue=4};
					break;

				}
			default:
				throw new Exception("SourceSize " + xSource + " not supported!");
			}
			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		}
	}
}