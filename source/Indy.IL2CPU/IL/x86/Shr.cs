using System;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
    // todo: Improve 8byte support
	[OpCode(OpCodeEnum.Shr)]
	public class Shr: Op {
        private string mLabelName;
		public Shr(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
            mLabelName= GetInstructionLabel(aReader);
		}
		public override void DoAssemble() {
            var xStackItem_ShiftAmount = Assembler.StackContents.Pop();
            var xStackItem_Value = Assembler.StackContents.Pop();
            if (xStackItem_Value.IsFloat) { throw new NotImplementedException("Floats not yet supported!"); }
            if (xStackItem_Value.Size <= 4)
            {
                new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX}; // shift amount
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX }; // value
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceReg = CPUx86.Registers.AL };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EBX, SourceReg=CPUx86.Registers.CL };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
                Assembler.StackContents.Push(xStackItem_Value);
                return;
            }
            if (xStackItem_Value.Size <= 8) {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue=0 };
                new CPU.Label(mLabelName + "__StartLoop");
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceReg=CPUx86.Registers.EAX};
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mLabelName + "__EndLoop" };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect=true };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceValue = 1 };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect=true, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceValue = 1 };
                new CPUx86.RotateThroughCarryRight { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, Size = 32, SourceReg=CPUx86.Registers.CL };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                new CPUx86.Jump { DestinationLabel = mLabelName + "__StartLoop" };

                new CPU.Label(mLabelName + "__EndLoop");
                Assembler.StackContents.Push(xStackItem_Value);
                return;
            }
            throw new NotImplementedException();
		}
	}
}