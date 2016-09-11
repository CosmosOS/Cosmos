using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Rem)]
  public class Rem : ILOp
  {
    public Rem(Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {

      var xStackItem = aOpCode.StackPopTypes[0];
      var xStackItemSize = SizeOfType(xStackItem);
      var xSize = Math.Max(xStackItemSize, SizeOfType(aOpCode.StackPopTypes[1]));

      if (xSize > 4)
      {
        if (TypeIsFloat(xStackItem))
        {
          XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
          XS.Add(ESP, 8);
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

          // shift the divident now in one step
          // shift divident CL bits right
          XS.ShiftRightDouble(EAX, EDX, CL);
          XS.ShiftRight(EDX, CL);

          // so we shifted both, so we have near the same relation as original values
          // divide this
          XS.IntegerDivide(ESI);

          // save result to stack
          XS.Push(0);
          XS.Push(EDX);

          //TODO: implement proper derivation correction and overflow detection

          XS.Jump(LabelEnd);

          XS.Label(LabelNoLoop);
          //save high dividend
          XS.Set(ECX, EAX);
          XS.Set(EAX, EDX);
          // extend that sign is in edx
          XS.SignExtendAX(RegisterSize.Int32);
          // divide high part
          XS.IntegerDivide(ESI);
          XS.Set(EAX, ECX);
          // divide low part
          XS.Divide(ESI);
          // save low result
          XS.Push(0);
          XS.Push(EDX);

          XS.Label(LabelEnd);
        }
      }
      else
      {
        if (TypeIsFloat(xStackItem))
        {
          XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
          XS.Add(ESP, 4);
          XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
          XS.Add(ESP, 4);
          XS.SSE.XorPS(XMM2, XMM2);
          XS.SSE.DivSS(XMM1, XMM0);
          XS.Sub(ESP, 4);
          XS.SSE.MoveSS(ESP, XMM2, destinationIsIndirect: true);
        }
        else
        {
          XS.Pop(ECX);
          XS.Pop(EAX); // gets devised by ecx
          XS.Xor(EDX, EDX);

          XS.Divide(ECX); // => EAX / ECX
          XS.Push(EDX);
        }
      }
    }
  }
}
