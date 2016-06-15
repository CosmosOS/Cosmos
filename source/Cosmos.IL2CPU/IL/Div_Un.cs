using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
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
					new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
					// value 2
					new CPUx86.x87.FloatDivide { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, Size = 64 };
					// override value 1
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
					// pop value 2
					XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 8);
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
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 4);

					//dividend
					// low
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 8);
					//high
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 12);

					// pop both 8 byte values
					XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 16);

					// set flags
					XS.Or(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI));
					// if high dword of divisor is already zero, we dont need the loop
					new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = LabelNoLoop };

					// set ecx to zero for counting the shift operations
					XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));

					new Label(LabelShiftRight);

					// shift divisor 1 bit right
					new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.EDI, ArgumentValue = 1 };
					XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 1);

					// increment shift counter
					XS.Increment(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));

					// set flags
					XS.Or(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI));
					// loop while high dword of divisor till it is zero
					new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelShiftRight };

					// shift the divident now in one step
					// shift divident CL bits right
					new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EDX, ArgumentReg = CPUx86.RegistersEnum.CL };
					XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.CL);

					// so we shifted both, so we have near the same relation as original values
					// divide this
					XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));

					// save result to stack
					new CPUx86.Push { DestinationValue = 0 };
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

					//TODO: implement proper derivation correction and overflow detection

					new CPUx86.Jump { DestinationLabel = LabelEnd };

					new Label(LabelNoLoop);

					//save high dividend
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
					// zero EDX, so that high part is zero -> reduce overflow case
					XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
					// divide high part
					XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
					// save high result
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
					// divide low part
					XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
					// save low result
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

					new Label(LabelEnd);
				}
            }
            else
            {
                if (TypeIsFloat(xStackItem))
                {
                    new MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                    new MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM1, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                    XS.SSE.MulSS(XSRegisters.XMM0, XSRegisters.XMM1);
                    new MoveSS { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.XMM1 };
                }
                else
                {
                    XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                }
            }
        }
    }
}
