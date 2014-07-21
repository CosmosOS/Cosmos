using System;
using CPUx86 = Cosmos.Assembler.x86;
using Label = Cosmos.Assembler.Label;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Div_Un )]
    public class Div_Un : ILOp
    {
        public Div_Un( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = aOpCode.StackPopTypes[0];
            var xStackItemSize = SizeOfType(xStackItem);
            var xStackItem2 = aOpCode.StackPopTypes[1];
            var xStackItem2Size = SizeOfType(xStackItem2);
            if( xStackItemSize == 8 )
            {
				// there seem to be an error in MS documentation, there is pushed an int32, but IL shows else
                if (xStackItem2Size != 8)
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Div.cs->Error: Expected a size of 8 for Div!");
                }
                if (TypeIsFloat(xStackItem))
				{
					// TODO add 0/0 infinity/infinity X/infinity
					// value 1
					new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
					// value 2
					new CPUx86.x87.FloatDivide { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
					// override value 1
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
					// pop value 2
					new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
				}
				else
				{
					string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
					string LabelShiftRight = BaseLabel + "ShiftRightLoop";
					string LabelNoLoop = BaseLabel + "NoLoop";
					string LabelEnd = BaseLabel + "End";

					// divisor
					//low
					new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
					//high
					new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

					//dividend
					// low
					new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 8 };
					//high
					new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 12 };

					// pop both 8 byte values
					new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 16 };

					// set flags
					new CPUx86.Or { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI };
					// if high dword of divisor is already zero, we dont need the loop
					new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = LabelNoLoop };

					// set ecx to zero for counting the shift operations
					new CPUx86.Xor { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ECX };

					new Label(LabelShiftRight);

					// shift divisor 1 bit right
					new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EDI, ArgumentValue = 1 };
					new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EDI, SourceValue = 1 };

					// increment shift counter
					new CPUx86.INC { DestinationReg = CPUx86.Registers.ECX };

					// set flags
					new CPUx86.Or { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI };
					// loop while high dword of divisor till it is zero
					new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelShiftRight };

					// shift the divident now in one step
					// shift divident CL bits right
					new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX, ArgumentReg = CPUx86.Registers.CL };
					new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.CL };

					// so we shifted both, so we have near the same relation as original values
					// divide this
					new CPUx86.Divide { DestinationReg = CPUx86.Registers.ESI };

					// save result to stack
					new CPUx86.Push { DestinationValue = 0 };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };

					//TODO: implement proper derivation correction and overflow detection

					new CPUx86.Jump { DestinationLabel = LabelEnd };

					new Label(LabelNoLoop);

					//save high dividend
					new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EAX };
					new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
					// zero EDX, so that high part is zero -> reduce overflow case
					new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
					// divide high part
					new CPUx86.Divide { DestinationReg = CPUx86.Registers.ESI };
					// save high result
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
					new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX };
					// divide low part
					new CPUx86.Divide { DestinationReg = CPUx86.Registers.ESI };
					// save low result
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };

					new Label(LabelEnd);
				}
            }
            else
            {
                if (TypeIsFloat(xStackItem))
                {
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.MulSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM1 };
                }
                else
                {
                    new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                }
            }
        }
    }
}