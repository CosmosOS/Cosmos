using System;

using CPUx86 = Cosmos.Assembler.x86;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

/* Div.Un is unsigned integer division so the valid input values are uint / ulong and the result is always expressed as unsigned */
namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Div_Un)]
    public class Div_Un : ILOp
    {
        public Div_Un(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xStackItem = aOpCode.StackPopTypes[0];
            var xStackItemSize = SizeOfType(xStackItem);
            var xStackItem2 = aOpCode.StackPopTypes[1];
            var xStackItem2Size = SizeOfType(xStackItem2);

            if (TypeIsFloat(xStackItem))
            {
                throw new Exception("Cosmos.IL2CPU.x86->IL->Div_Un.cs->Error: Expected unsigned integer operands but get float!");
            }

            if (xStackItemSize == 8)
            {
                // there seem to be an error in MS documentation, there is pushed an int32, but IL shows else
                if (xStackItem2Size != 8)
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Div.cs->Error: Expected a size of 8 for Div!");
                }

                // ulong
                string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
                string LabelShiftRight = BaseLabel + "ShiftRightLoop";
                string LabelNoLoop = BaseLabel + "NoLoop";
                string LabelEnd = BaseLabel + "End";

                // divisor
                // low
                XS.Set(ESI, ESP, sourceIsIndirect: true);
                // high
                XS.Set(EDI, ESP, sourceDisplacement: 4);

                XS.Add(ESP, 8);

                // dividend
                // low
                XS.Set(EAX, ESP, sourceIsIndirect: true);
                // high
                XS.Set(EDX, ESP, sourceDisplacement: 4);

                XS.Add(ESP, 8);

                // set flags
                XS.Or(EDI, EDI);
                // if high dword of divisor is already zero, we dont need the loop
                XS.Jump(CPUx86.ConditionalTestEnum.Zero, LabelNoLoop);

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
                XS.Jump(CPUx86.ConditionalTestEnum.NotZero, LabelShiftRight);

                // shift the dividend now in one step
                // shift dividend CL bits right
                XS.ShiftRightDouble(EAX, EDX, CL);
                XS.ShiftRight(EDX, CL);

                // so we shifted both, so we have near the same relation as original values
                // divide this
                XS.Divide(ESI);

                // save result to stack
                XS.Push(0);
                XS.Push(EAX);

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
                // save high result
                XS.Push(EAX);
                XS.Set(EAX, ECX);
                // divide low part
                XS.Divide(ESI);
                // save low result
                XS.Push(EAX);

                XS.Label(LabelEnd);
            }
            else
            {
                XS.Xor(EDX, EDX);
                XS.Pop(ECX);
                XS.Pop(EAX);
                XS.Divide(ECX);
                XS.Push(EAX);
            }
        }
    }
}
