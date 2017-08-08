using System;
using Cosmos.Assembler.x86.SSE;

using XSharp.Common;
using static XSharp.Common.XSRegisters;
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

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = aOpCode.StackPopTypes[0];
            var xStackItemSize = SizeOfType(xStackItem);
            var xSize = Math.Max(xStackItemSize, SizeOfType(aOpCode.StackPopTypes[1]));
            if (xSize > 4)
            {
                if (TypeIsFloat(xStackItem))
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.ESP, 8);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE.XorPS(XMM2, XMM2);
                    XS.SSE.DivPS(XMM1, XMM0);
                    XS.SSE.MoveSS(ESP, XMM2, destinationIsIndirect: true);
                }
                else
                {
					string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
					string LabelShiftRight = BaseLabel + "ShiftRightLoop";
					string LabelNoLoop = BaseLabel + "NoLoop";
					string LabelEnd = BaseLabel + "End";

					// divisor
					//low
					XS.Set(ESI, ESP, sourceIsIndirect: true);
					//high
					XS.Set(XSRegisters.EDI, XSRegisters.ESP, sourceDisplacement: 4);

					//dividend
					// low
					XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceDisplacement: 8);
					//high
					XS.Set(XSRegisters.EDX, XSRegisters.ESP, sourceDisplacement: 12);

					// pop both 8 byte values
					XS.Add(XSRegisters.ESP, 16);

					// set flags
					XS.Or(XSRegisters.EDI, XSRegisters.EDI);
					// if high dword of divisor is already zero, we dont need the loop
					XS.Jump(CPUx86.ConditionalTestEnum.Zero, LabelNoLoop);

					// set ecx to zero for counting the shift operations
					XS.Xor(XSRegisters.ECX, XSRegisters.ECX);

					XS.Label(LabelShiftRight);

					// shift divisor 1 bit right
          XS.ShiftRightDouble(ESI, EDI, 1);
					XS.ShiftRight(XSRegisters.EDI, 1);

					// increment shift counter
					XS.Increment(XSRegisters.ECX);

					// set flags
					XS.Or(XSRegisters.EDI, XSRegisters.EDI);
					// loop while high dword of divisor till it is zero
					XS.Jump(CPUx86.ConditionalTestEnum.NotZero, LabelShiftRight);

					// shift the divident now in one step
					// shift divident CL bits right
          XS.ShiftRightDouble(EAX, EDX, CL);
					XS.ShiftRight(XSRegisters.EDX, CL);

					// so we shifted both, so we have near the same relation as original values
					// divide this
					XS.Divide(XSRegisters.ESI);

					// save remainder to stack
					XS.Push(0);
					XS.Push(XSRegisters.EDX);

					//TODO: implement proper derivation correction and overflow detection

					XS.Jump(LabelEnd);

					XS.Label(LabelNoLoop);

					//save high dividend
					XS.Set(XSRegisters.ECX, XSRegisters.EAX);
					XS.Set(XSRegisters.EAX, XSRegisters.EDX);
					// zero EDX, so that high part is zero -> reduce overflow case
					XS.Xor(XSRegisters.EDX, XSRegisters.EDX);
					// divide high part
					XS.Divide(XSRegisters.ESI);
					XS.Set(XSRegisters.EAX, XSRegisters.ECX);
					// divide low part
					XS.Divide(XSRegisters.ESI);
					// save remainder result
					XS.Push(0);
					XS.Push(XSRegisters.EDX);

					XS.Label(LabelEnd);
                }
            }
            else
            {
                if (TypeIsFloat(xStackItem))
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.ESP, 4);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.ESP, 4);
                    XS.SSE.XorPS(XMM2, XMM2);
                    XS.SSE.DivPS(XMM1, XMM0);
                    XS.Sub(XSRegisters.ESP, 4);
                    XS.SSE.MoveSS(ESP, XMM2, destinationIsIndirect: true);
                }
                else
                {
                    XS.Pop(XSRegisters.ECX);
                    XS.Pop(XSRegisters.EAX); // gets devised by ecx
                    XS.Xor(XSRegisters.EDX, XSRegisters.EDX);

                    XS.Divide(XSRegisters.ECX); // => EAX / ECX
                    XS.Push(XSRegisters.EDX);
                }
            }
        }
    }
}
