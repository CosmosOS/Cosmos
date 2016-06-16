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
                    XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 8);
                    XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.SSE.XorPS(XSRegisters.XMM2, XSRegisters.XMM2);
                    XS.SSE.DivPS(XSRegisters.XMM1, XSRegisters.XMM0);
                    XS.SSE.MoveSS(XSRegisters.ESP, XSRegisters.XMM2, destinationIsIndirect: true);
                }
                else
                {
					string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
					string LabelShiftRight = BaseLabel + "ShiftRightLoop";
					string LabelNoLoop = BaseLabel + "NoLoop";
					string LabelEnd = BaseLabel + "End";

					// divisor
					//low
					XS.Set(XSRegisters.ESI, XSRegisters.ESP, sourceIsIndirect: true);
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
					XS.Jump(CPUx86.ConditionalTestEnum.Zero, LabelNoLoop);

					// set ecx to zero for counting the shift operations
					XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));

					XS.Label(LabelShiftRight);

					// shift divisor 1 bit right
					new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.EDI, ArgumentValue = 1 };
					XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 1);

					// increment shift counter
					XS.Increment(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));

					// set flags
					XS.Or(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI));
					// loop while high dword of divisor till it is zero
					XS.Jump(CPUx86.ConditionalTestEnum.NotZero, LabelShiftRight);

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

					XS.Jump(LabelEnd);

					XS.Label(LabelNoLoop);

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

					XS.Label(LabelEnd);
                }
            }
            else
            {
                if (TypeIsFloat(xStackItem))
                {
                    XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                    XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                    XS.SSE.XorPS(XSRegisters.XMM2, XSRegisters.XMM2);
                    XS.SSE.DivPS(XSRegisters.XMM1, XSRegisters.XMM0);
                    XS.Sub(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                    XS.SSE.MoveSS(XSRegisters.ESP, XSRegisters.XMM2, destinationIsIndirect: true);
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
