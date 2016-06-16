using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;

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
        if (xStackItemIsFloat)
        {
          XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
          XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
          XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
          new CompareSS { DestinationReg = RegistersEnum.XMM1, SourceReg = RegistersEnum.XMM0, pseudoOpcode = (byte)ComparePseudoOpcodes.Equal };
          XS.SSE2.MoveD(XSRegisters.XMM1, XSRegisters.EBX);
          XS.And(XSRegisters.OldToNewRegister(RegistersEnum.EBX), 1);
          new Mov { SourceReg = RegistersEnum.EBX, DestinationReg = RegistersEnum.ESP, DestinationIsIndirect = true };
        }
        else
        {
          XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
          XS.Compare(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
          XS.Jump(ConditionalTestEnum.Equal, Label.LastFullLabel + ".True");
          XS.Jump(Label.LastFullLabel + ".False");
          XS.Label(".True");
          XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
          XS.Push(1);
          XS.Jump(xNextLabel);
          XS.Label(".False");
          XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
          XS.Push(0);
          XS.Jump(xNextLabel);
        }
      }
      else if (xSize > 4)
      {
        if (xStackItemIsFloat)
        {
          XS.Set(XSRegisters.OldToNewRegister(RegistersEnum.ESI), 1);
          // esi = 1
          XS.Xor(XSRegisters.OldToNewRegister(RegistersEnum.EDI), XSRegisters.OldToNewRegister(RegistersEnum.EDI));
          // edi = 0

          // value 1
          new FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationDisplacement = 8, DestinationIsIndirect = true };
          // value 2
          new FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
          XS.FPU.FloatCompareAndSet(XSRegisters.ST1);
          // if zero is set, ST(0) == ST(i)
          new ConditionalMove { Condition = ConditionalTestEnum.Equal, DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.ESI };
          // pops fpu stack
          XS.FPU.FloatStoreAndPop(XSRegisters.ST0);
          XS.FPU.FloatStoreAndPop(XSRegisters.ST0);
          XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 16);
          XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.EDI));
        }
        else
        {
          XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
          XS.Compare(XSRegisters.OldToNewRegister(RegistersEnum.EAX), XSRegisters.OldToNewRegister(RegistersEnum.ESP), sourceDisplacement: 4);
          XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
          XS.Jump(ConditionalTestEnum.NotEqual, Label.LastFullLabel + ".False");
          XS.Xor(XSRegisters.OldToNewRegister(RegistersEnum.EAX), XSRegisters.OldToNewRegister(RegistersEnum.ESP), sourceDisplacement: 4);
          XS.Jump(ConditionalTestEnum.NotZero, Label.LastFullLabel + ".False");

          //they are equal, eax == 0
          XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 8);
          XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.EAX), 1);
          XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
          XS.Jump(xNextLabel);
          XS.Label(Label.LastFullLabel + ".False");
          //eax = 0
          XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 8);
          XS.Xor(XSRegisters.OldToNewRegister(RegistersEnum.EAX), XSRegisters.OldToNewRegister(RegistersEnum.EAX));
          XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
          new CPUx86.Jump { DestinationLabel = xNextLabel };
        }

      }
      else
      {
        throw new Exception("Cosmos.IL2CPU.x86->IL->Ceq.cs->Error: Case not handled!");
      }
    }
  }
}
