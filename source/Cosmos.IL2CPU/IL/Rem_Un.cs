using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
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
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 8);
                    new MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM1, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                    new XorPS { DestinationReg = CPUx86.RegistersEnum.XMM2, SourceReg = CPUx86.RegistersEnum.XMM2 };
                    new DivPS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.XMM1 };
                    new MoveSS { SourceReg = CPUx86.RegistersEnum.XMM2, DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
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

					//dividend
					// low
					new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 8 };
					//high
					new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EDX, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 12 };

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

					// save remainder to stack
					new CPUx86.Push { DestinationValue = 0 };
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));

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
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
					// divide low part
					XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
					// save remainder result
					new CPUx86.Push { DestinationValue = 0 };
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));

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
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                    new XorPS { DestinationReg = CPUx86.RegistersEnum.XMM2, SourceReg = CPUx86.RegistersEnum.XMM2 };
                    new DivPS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.XMM1 };
                    XS.Sub(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                    new MoveSS { SourceReg = CPUx86.RegistersEnum.XMM2, DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
                }
                else
                {
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX)); // gets devised by ecx
                    XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));

                    XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX)); // => EAX / ECX
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
                }
            }
        }
    }
}
