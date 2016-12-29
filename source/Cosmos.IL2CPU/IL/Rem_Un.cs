using System;

using Cosmos.Assembler.x86;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Divides two unsigned values and pushes the remainder onto the evaluation stack.
    /// </summary>
    [OpCode( ILOpCode.Code.Rem_Un )]
    public class Rem_Un : ILOp
    {
#warning TODO: Improve remainder on UInt64, in the case of high dword of divisor not 0
        public Rem_Un( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = aOpCode.StackPopTypes[0];
            var xSize = Math.Max(SizeOfType(xStackItem), SizeOfType(aOpCode.StackPopTypes[1]));

            if (TypeIsFloat(xStackItem))
            {
              throw new Exception("Cosmos.IL2CPU.x86->IL->Rem_Un.cs->Error: Expected unsigned integer operands but got float!");
            }

            if (xSize > 8)
            {
              throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Rem_Un.cs->Error: StackSize > 8 not supported");
            }
            else if (xSize > 4) //ulong
            {
					      string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
					      string LabelShiftRight = BaseLabel + "ShiftRightLoop";
					      string LabelNoLoop = BaseLabel + "NoLoop";
					      string LabelEnd = BaseLabel + "End";

					      // divisor
					      //low
					      XS.Set(ESI, ESP, sourceIsIndirect: true);
					      //high
					      XS.Set(EDI, ESP, sourceDisplacement: 4);

					      //dividend
					      // low
					      XS.Set(EAX, ESP, sourceDisplacement: 8);
					      //high
					      XS.Set(EDX, ESP, sourceDisplacement: 12);

					      // pop both 8 byte values
					      XS.Add(ESP, 16);

					      // set flags
					      XS.Or(EDI, EDI);
					      // if high dword of divisor is already zero, we dont need the loop
					      XS.Jump(ConditionalTestEnum.Zero, LabelNoLoop);

					      // set ecx to zero for counting the shift operations
					      XS.Xor(ECX, ECX);

					      XS.Label(LabelShiftRight);

					      // shift divisor 1 bit right
                XS.ShiftRightDouble(ESI, EDI, 1);
					      XS.ShiftRight(EDI, 1);

					      // increment shift counter
					      XS.Increment(ECX);

					      // set flags
					      XS.Or(EDI, EDI);
					      // loop while high dword of divisor till it is zero
					      XS.Jump(ConditionalTestEnum.NotZero, LabelShiftRight);

					      // shift the dividend now in one step
                XS.ShiftRightDouble(EAX, EDX, CL);
					      // shift dividend CL bits right
					      XS.ShiftRight(EDX, CL);

					      // so we shifted both, so we have near the same relation as original values
					      // divide this
					      XS.Divide(ESI);

                // set eax to zero
                XS.Xor(EAX, EAX);

                // shift the remainder in one step
                XS.ShiftLeftDouble(EAX, EDX, CL);
                // shift lower dword of remainder CL bits left
                XS.ShiftLeft(EDX, CL);

					      // save remainder to stack
					      XS.Push(EAX);
					      XS.Push(EDX);

					      //TODO: implement proper derivation correction and overflow detection

					      XS.Jump(LabelEnd);

					      XS.Label(LabelNoLoop);

					      //save high dividend
					      XS.Set(ECX, EAX);
					      XS.Set(EAX, EDX);

                // zero EDX, so that high part is zero -> reduce overflow case
					      XS.Xor(EDX, EDX);

                // divide high part
					      XS.Divide(ESI);
					      XS.Set(EAX, ECX);

					      // divide low part
					      XS.Divide(ESI);

                // save remainder result
					      XS.Push(0);
					      XS.Push(EDX);

					      XS.Label(LabelEnd);
            }
            else //uint
            {
                // divisor
                XS.Pop(ECX);
                // dividend
                XS.Pop(EAX);

                XS.Xor(EDX, EDX);

                XS.Divide(ECX); // => EAX / ECX
                XS.Push(EDX);
            }
        }
    }
}
