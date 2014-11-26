using System;
using CPUx86 = Cosmos.Assembler.x86;
using Label = Cosmos.Assembler.Label;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Divides two unsigned values and pushes the remainder onto the evaluation stack.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Rem_Un )]
    public class Rem_Un : ILOp
    {
        public Rem_Un( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = aOpCode.StackPopTypes[0];
            var xStackItemSize = SizeOfType(xStackItem);
            var xSize = Math.Max(xStackItemSize, SizeOfType(aOpCode.StackPopTypes[1]));
            if (xSize > 4)
            {
                if (TypeIsFloat(xStackItem))
                {
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.XorPS { DestinationReg = CPUx86.Registers.XMM2, SourceReg = CPUx86.Registers.XMM2 };
                    new CPUx86.SSE.DivPS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.XMM1 };
                    new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.XMM2, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
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

					// save remainder to stack
					new CPUx86.Push { DestinationValue = 0 };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };

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
					new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX };
					// divide low part
					new CPUx86.Divide { DestinationReg = CPUx86.Registers.ESI };
					// save remainder result
					new CPUx86.Push { DestinationValue = 0 };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };

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
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.SSE.XorPS { DestinationReg = CPUx86.Registers.XMM2, SourceReg = CPUx86.Registers.XMM2 };
                    new CPUx86.SSE.DivPS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.XMM1 };
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.XMM2, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                }
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // gets devised by ecx
                    new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };

                    new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX }; // => EAX / ECX 
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
                }
            }
        }
    }
}