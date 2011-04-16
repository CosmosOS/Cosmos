using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler.X86;
using Label = Cosmos.Compiler.Assembler.Label;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Div_Un )]
    public class Div_Un : ILOp
    {
        public Div_Un( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = Assembler.Stack.Pop();
            if( xStackItem.Size == 8 )
            {
				// there seem to be an error in MS documentation, there is pushed an int32, but IL shows else
				if (Assembler.Stack.Pop().Size != 8)
					throw new Exception("Expect a size of 8 for Div!");
				if (xStackItem.IsFloat)
				{// TODO add 0/0 infinity/infinity X/infinity
					// value 1
					new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
					// value 2
					new CPUx86.x87.FloatDivide { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
					// override value 1
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
					// pop value 2
					new CPUx86.Add { DestinationReg = Registers.ESP, SourceValue = 8 };
					Assembler.Stack.Push(8, typeof(Double));
				}
				else
				{
					string BaseLabel = GetLabel(aMethod, aOpCode) + "__";
					string LabelShiftRight = BaseLabel + "ShiftRightLoop";
					string LabelNoLoop = BaseLabel + "NoLoop";

					// divisor
					//low
					new CPUx86.Move { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
					//high
					new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

					//dividend
					// low
					new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 8 };
					//high
					new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 12 };

					// set flags
					new CPUx86.Or { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI };
					// if high dword of divisor is already zero, we dont need the loop
					new CPUx86.ConditionalJump { Condition = ConditionalTestEnum.Zero, DestinationLabel = LabelNoLoop };

					// set ecx to zero for counting the shift operations
					new CPUx86.Xor { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ECX };

					new Label(LabelShiftRight);

					// shift divisor 1 bit right
					new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EDI, ArgumentValue = 1 };
					new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EDI, SourceValue = 1 };

					// increment shift counter
					new CPUx86.Inc { DestinationReg = CPUx86.Registers.ECX };

					// set flags
					new CPUx86.Or { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI };
					// loop while high dword of divisor till it is zero
					new CPUx86.ConditionalJump { Condition = ConditionalTestEnum.NotZero, DestinationLabel = LabelShiftRight };

					// shift the divident now in one step
					// shift divident CL bits right
					new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX, ArgumentReg = CPUx86.Registers.CL };
					new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.CL };

					new Label(LabelNoLoop);

					// so we shifted both, so we have near the same relation as original values
					// divide this
					new CPUx86.Divide { DestinationReg = CPUx86.Registers.ESI };

					// pop both 8 byte values
					new CPUx86.Add { DestinationReg = Registers.ESP, SourceValue = 16 };

					// save result to stack
					new CPUx86.Push { DestinationValue = 0 };
					new CPUx86.Push { DestinationReg = Registers.EAX };

					//TODO: implement proper derivation correction and overflow detection

					Assembler.Stack.Push(8, typeof(ulong));
				}
            }
            else
            {
                if (xStackItem.IsFloat)
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