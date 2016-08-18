using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using static Cosmos.Assembler.x86.SSE.ComparePseudoOpcodes;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ceq)]
  public class Ceq : ILOp {
    public Ceq(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
        var xStackItem = aOpCode.StackPopTypes[0];
        var xStackItemSize = SizeOfType(xStackItem);
        var xStackItemIsFloat = TypeIsFloat(xStackItem);
        var xStackItem2 = aOpCode.StackPopTypes[1];
        var xStackItem2Size = SizeOfType(xStackItem2);
        var xStackItem2IsFloat = TypeIsFloat(xStackItem2);
      var xSize = Math.Max(xStackItemSize, xStackItem2Size);

      var xNextLabel = GetLabel(aMethod, aOpCode.NextPosition);

      if (xSize > 8)
      {
        throw new Exception("Cosmos.IL2CPU.x86->IL->Ceq.cs->Error: StackSizes > 8 not supported");
      }
      else if (xSize <= 4)
      {
        if (xStackItemIsFloat) // float
        {
          XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
          XS.Add(ESP, 4);
          XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
          XS.SSE.CompareSS(XMM1, XMM0, comparision: Equal);
          XS.SSE2.MoveD(EBX, XMM1);
          XS.And(EBX, 1);
          XS.Set(ESP, EBX, destinationIsIndirect: true);
        }
        else
        {
          XS.Pop(EAX);
          XS.Compare(EAX, ESP, sourceIsIndirect: true);
          XS.Jump(ConditionalTestEnum.Equal, Label.LastFullLabel + ".True");
          XS.Jump(Label.LastFullLabel + ".False");
          XS.Label(".True");
          XS.Add(ESP, 4);
          XS.Push(1);
          XS.Jump(xNextLabel);
          XS.Label(".False");
          XS.Add(ESP, 4);
          XS.Push(0);
          XS.Jump(xNextLabel);
        }
      }
      else if (xSize > 4)
      {
        if (xStackItemIsFloat)
        {
           // Please note that SSE supports double operations only from version 2
           XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
           // Increment ESP to get the value of the next double
           XS.Add(ESP, 8);
           XS.SSE2.MoveSD(XMM1, ESP, sourceIsIndirect: true);
           XS.SSE2.CompareSD(XMM1, XMM0, comparision: Equal);
           XS.SSE2.MoveD(EBX, XMM1);
           XS.And(EBX, 1);
           // We need to move the stack pointer of 4 Byte to "eat" the second double that is yet in the stack or we get a corrupted stack!
           XS.Add(ESP, 4);
           XS.Set(ESP, EBX, destinationIsIndirect: true);
           // This is the x87 version I left it here commented if in future will be needed...
#if false
          XS.Set(XSRegisters.ESI, 1);
          // esi = 1
          XS.Xor(XSRegisters.EDI, XSRegisters.EDI);
          // edi = 0

          // value 1
          new FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationDisplacement = 8, DestinationIsIndirect = true };
          // value 2
          XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Long64);
          XS.FPU.FloatCompareAndSet(ST1);
          // if zero is set, ST(0) == ST(i)
          new ConditionalMove { Condition = ConditionalTestEnum.Equal, DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.ESI };
          // pops fpu stack
          XS.FPU.FloatStoreAndPop(ST0);
          XS.FPU.FloatStoreAndPop(ST0);
          XS.Add(XSRegisters.ESP, 16);
          XS.Push(XSRegisters.EDI);
#endif
        }
        else
        {
          XS.Pop(EAX);
          XS.Compare(EAX, ESP, sourceDisplacement: 4);
          XS.Pop(EAX);
          XS.Jump(ConditionalTestEnum.NotEqual, Label.LastFullLabel + ".False");
          XS.Xor(EAX, ESP, sourceDisplacement: 4);
          XS.Jump(ConditionalTestEnum.NotZero, Label.LastFullLabel + ".False");

          //they are equal, eax == 0
          XS.Add(ESP, 8);
          XS.Add(EAX, 1);
          XS.Push(EAX);
          XS.Jump(xNextLabel);
          XS.Label(Label.LastFullLabel + ".False");
          //eax = 0
          XS.Add(ESP, 8);
          XS.Xor(EAX, EAX);
          XS.Push(EAX);
          XS.Jump(xNextLabel);
        }

      }
      else
      {
        throw new Exception("Cosmos.IL2CPU.x86->IL->Ceq.cs->Error: Case not handled!");
      }
    }
  }
}
