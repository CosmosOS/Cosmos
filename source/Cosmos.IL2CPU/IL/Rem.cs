using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Rem)]
	public class Rem: ILOp
	{
		public Rem(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {

        var xStackItem = aOpCode.StackPopTypes[0];
        var xStackItemSize = SizeOfType(xStackItem);
        var xSize = Math.Max(xStackItemSize, SizeOfType(aOpCode.StackPopTypes[1]));
        if( xSize > 4 )
        {
            if (TypeIsFloat( xStackItem))
            {
                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, SourceValue = 8 };
                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM1, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                new CPUx86.SSE.XorPS { DestinationReg = CPUx86.RegistersEnum.XMM2, SourceReg = CPUx86.RegistersEnum.XMM2 };
                new CPUx86.SSE.DivPS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.XMM1 };
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.RegistersEnum.XMM2, DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
            }
            else
            {
				string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
				string LabelShiftRight = BaseLabel + "ShiftRightLoop";
				string LabelNoLoop = BaseLabel + "NoLoop";
				string LabelEnd = BaseLabel + "End";

				// divisor
				//low
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
				//high
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

				// pop both 8 byte values
				new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, SourceValue = 16 };

				//dividend
				// low
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 8 };
				//high
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDX, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 12 };

				// set flags
				new CPUx86.Or { DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.EDI };
				// if high dword of divisor is already zero, we dont need the loop
				new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = LabelNoLoop };

				// set ecx to zero for counting the shift operations
				new CPUx86.Xor { DestinationReg = CPUx86.RegistersEnum.ECX, SourceReg = CPUx86.RegistersEnum.ECX };

				new Label(LabelShiftRight);

				// shift divisor 1 bit right
				new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.EDI, ArgumentValue = 1 };
				new CPUx86.ShiftRight { DestinationReg = CPUx86.RegistersEnum.EDI, SourceValue = 1 };

				// increment shift counter
				new CPUx86.INC { DestinationReg = CPUx86.RegistersEnum.ECX };

				// set flags
				new CPUx86.Or { DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.EDI };
				// loop while high dword of divisor till it is zero
				new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelShiftRight };

				// shift the divident now in one step
				// shift divident CL bits right
				new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EDX, ArgumentReg = CPUx86.RegistersEnum.CL };
				new CPUx86.ShiftRight { DestinationReg = CPUx86.RegistersEnum.EDX, SourceReg = CPUx86.RegistersEnum.CL };

				// so we shifted both, so we have near the same relation as original values
				// divide this
				new CPUx86.IDivide { DestinationReg = CPUx86.RegistersEnum.ESI };

				// save result to stack
				new CPUx86.Push { DestinationValue = 0 };
				new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EDX };

				//TODO: implement proper derivation correction and overflow detection

				new CPUx86.Jump { DestinationLabel = LabelEnd };

				new Label(LabelNoLoop);
				//save high dividend
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
				// extend that sign is in edx
				new CPUx86.SignExtendAX { Size = 32 };
				// divide high part
				new CPUx86.IDivide { DestinationReg = CPUx86.RegistersEnum.ESI };
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
				// divide low part
				new CPUx86.Divide { DestinationReg = CPUx86.RegistersEnum.ESI };
				// save low result
				new CPUx86.Push { DestinationValue = 0};
				new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EDX };

				new Label(LabelEnd);
            }
        }
        else
        {
            if (TypeIsFloat(xStackItem))
            {
                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, SourceValue = 4 };
                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM1, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, SourceValue = 4 };
                new CPUx86.SSE.XorPS { DestinationReg = CPUx86.RegistersEnum.XMM2, SourceReg = CPUx86.RegistersEnum.XMM2 };
                new CPUx86.SSE.DivSS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.XMM1 };
                new CPUx86.Sub { DestinationReg = CPUx86.RegistersEnum.ESP, SourceValue = 4 };
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.RegistersEnum.XMM2, DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
            }
            else
            {
                new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.ECX };
                new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EAX }; // gets devised by ecx
                new CPUx86.Xor { DestinationReg = CPUx86.RegistersEnum.EDX, SourceReg = CPUx86.RegistersEnum.EDX };

                new CPUx86.Divide { DestinationReg = CPUx86.RegistersEnum.ECX }; // => EAX / ECX
                new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EDX };
            }
        }
     }
	}
}
