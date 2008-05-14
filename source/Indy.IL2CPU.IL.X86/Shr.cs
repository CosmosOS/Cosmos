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
                new CPUx86.Pop(CPUx86.Registers.EAX); // shift amount
                new CPUx86.Pop(CPUx86.Registers.EBX); // value
                new CPUx86.Move(CPUx86.Registers.CL, CPUx86.Registers.AL);
                new CPUx86.ShiftRight(CPUx86.Registers.EBX, CPUx86.Registers.CL);
                new CPUx86.Pushd(CPUx86.Registers.EBX);
                Assembler.StackContents.Push(xStackItem_Value);
                return;
            }
            if (xStackItem_Value.Size <= 8) {
                new CPUx86.Pop("edx");
                new CPUx86.Move("eax", "0");
                new CPU.Label(mLabelName + "__StartLoop");
                new CPUx86.Compare("edx", "eax");
                new CPUx86.JumpIfEqual(mLabelName + "__EndLoop");
                new CPUx86.Move("ebx", "[esp]");
                new CPUx86.Move("CL", "1");
                new CPUx86.ShiftRight("ebx", "CL");
                new CPUx86.Move("[esp]", "ebx");
                new CPUx86.Move("CL", "1");
                new CPUx86.RotateThroughCarryRight("dword [esp+4]");
                new CPUx86.Add("eax", "1");
                new CPUx86.Jump(mLabelName + "__StartLoop");

                new CPU.Label(mLabelName + "__EndLoop");
                Assembler.StackContents.Push(xStackItem_Value);
                return;
            }
            throw new NotImplementedException();
		}
	}
}