using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

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
          new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
          new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
          new CPUx86.SSE.CompareSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0, pseudoOpcode = (byte)CPUx86.SSE.ComparePseudoOpcodes.Equal };
          new CPUx86.MoveD { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.XMM1 };
          new CPUx86.And { DestinationReg = CPUx86.Registers.EBX, SourceValue = 1 };
          new CPUx86.Mov { SourceReg = CPUx86.Registers.EBX, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
        }
        else // integer XXX this seems more code that should be needed! Maybe using MMX PQMxxx could be better?
        {
          new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
          new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
          new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = Label.LastFullLabel + ".True" };
          new CPUx86.Jump { DestinationLabel = Label.LastFullLabel + ".False" };
          new Label(".True");
          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
          new CPUx86.Push { DestinationValue = 1 };
          new CPUx86.Jump { DestinationLabel = xNextLabel };
          new Label(".False");
          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
          new CPUx86.Push { DestinationValue = 0 };
          new CPUx86.Jump { DestinationLabel = xNextLabel };
        }
      } 
      else if (xSize > 4)
      {
        if (xStackItemIsFloat) 
        {
           // Please note that SSE supports double operations only from version 2
           new CPUx86.SSE.MoveSD { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
           new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
           new CPUx86.SSE.MoveSD { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
           new CPUx86.SSE.CompareSD { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0, pseudoOpcode = (byte)CPUx86.SSE.ComparePseudoOpcodes.Equal };
           new CPUx86.MoveD { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.XMM1 };
           new CPUx86.And { DestinationReg = CPUx86.Registers.EBX, SourceValue = 1 };
           // We need to move the stack pointer of 4 Byte to "eat" the second double that is yet in the stack or we get a corrupted stack!
           new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
           new CPUx86.Mov { SourceReg = CPUx86.Registers.EBX, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
#if false
          // esi = 1
          new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI };
          // edi = 0

          // value 1
          new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationDisplacement = 8, DestinationIsIndirect = true };
          // value 2
          new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
          new CPUx86.x87.FloatCompareAndSet { DestinationReg = Registers.ST1 };
          // if zero is set, ST(0) == ST(i)
          new CPUx86.ConditionalMove { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESI };
          // pops fpu stack
          new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ST0 };
          new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ST0 };
          new CPUx86.Add { DestinationReg = Registers.ESP, SourceValue = 16 };
          new CPUx86.Push { DestinationReg = CPUx86.Registers.EDI }; 
#endif
        }
        else 
        {
          new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
          new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };
          new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
          new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = Label.LastFullLabel + ".False" };
          new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };
          new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = Label.LastFullLabel + ".False" };

          //they are equal, eax == 0
          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
          new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
          new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
          new CPUx86.Jump { DestinationLabel = xNextLabel };
          new Label(Label.LastFullLabel + ".False");
          //eax = 0
          new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
          new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };
          new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
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