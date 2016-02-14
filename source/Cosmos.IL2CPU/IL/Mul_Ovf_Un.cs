using System;

using Cosmos.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Mul_Ovf_Un)]
  public class Mul_Ovf_Un : ILOp
  {
    public Mul_Ovf_Un(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xStackContent = aOpCode.StackPopTypes[0];
      var xStackContentSize = SizeOfType(xStackContent);
      string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
      DoExecute(xStackContentSize, BaseLabel);
    }

    public static void DoExecute(uint xStackContentSize, string aBaseLabel)
    {
      //TODO: Throw System.OverflowException if the result won't fit in the type.

      if (xStackContentSize > 4)
      {
        // div of both == LEFT_LOW * RIGHT_LOW + ((LEFT_LOW * RIGHT_HIGH + RIGHT_LOW * LEFT_HIGH) << 32)
        string Simple32Multiply = aBaseLabel + "Simple32Multiply";
        string MoveReturnValue = aBaseLabel + "MoveReturnValue";

        // right value
        // low
        //  SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true
        // high
        //  SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4

        // left value
        // low
        //  SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 8
        // high
        //  SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 12

        // compair LEFT_HIGH, RIGHT_HIGH , on zero only simple multiply is used
        //mov RIGHT_HIGH to eax, is useable on Full 64 multiply
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.EAX,
            SourceReg = Cosmos.Assembler.x86.Registers.ESP,
            SourceIsIndirect = true,
            SourceDisplacement = 4
          };
        new Assembler.x86.Or
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.EAX,
            SourceReg = Cosmos.Assembler.x86.Registers.ESP,
            SourceIsIndirect = true,
            SourceDisplacement = 12
          };
        new Assembler.x86.ConditionalJump
          {
            Condition = Cosmos.Assembler.x86.ConditionalTestEnum.Zero,
            DestinationLabel = Simple32Multiply
          };
        // Full 64 Multiply

        // copy again, or could change EAX
        //TODO is there an opcode that does OR without change EAX?
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.EAX,
            SourceReg = Cosmos.Assembler.x86.Registers.ESP,
            SourceIsIndirect = true,
            SourceDisplacement = 4
          };
        // eax contains already RIGHT_HIGH
        // multiply with LEFT_LOW
        new Assembler.x86.Multiply
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ESP,
            DestinationIsIndirect = true,
            DestinationDisplacement = 8,
            Size = 32
          };
        // save result of LEFT_LOW * RIGHT_HIGH
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ECX,
            SourceReg = Cosmos.Assembler.x86.Registers.EAX
          };

        //mov RIGHT_LOW to eax
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.EAX,
            SourceReg = Cosmos.Assembler.x86.Registers.ESP,
            SourceIsIndirect = true
          };
        // multiply with LEFT_HIGH
        new Assembler.x86.Multiply
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ESP,
            DestinationIsIndirect = true,
            DestinationDisplacement = 12,
            Size = 32
          };
        // add result of LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH
        new Assembler.x86.Add
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ECX,
            SourceReg = Cosmos.Assembler.x86.Registers.EAX
          };

        //mov RIGHT_LOW to eax
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.EAX,
            SourceReg = Cosmos.Assembler.x86.Registers.ESP,
            SourceIsIndirect = true
          };
        // multiply with LEFT_LOW
        new Assembler.x86.Multiply
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ESP,
            DestinationIsIndirect = true,
            DestinationDisplacement = 8,
            Size = 32
          };
        // add LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH to high dword of last result
        new Assembler.x86.Add
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.EDX,
            SourceReg = Cosmos.Assembler.x86.Registers.ECX
          };

        new Assembler.x86.Jump { DestinationLabel = MoveReturnValue };

        new Label(Simple32Multiply);
        //mov RIGHT_LOW to eax
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.EAX,
            SourceReg = Cosmos.Assembler.x86.Registers.ESP,
            SourceIsIndirect = true
          };
        // multiply with LEFT_LOW
        new Assembler.x86.Multiply
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ESP,
            DestinationIsIndirect = true,
            DestinationDisplacement = 8,
            Size = 32
          };

        new Label(MoveReturnValue);
        // move high result to left high
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ESP,
            DestinationIsIndirect = true,
            DestinationDisplacement = 12,
            SourceReg = Cosmos.Assembler.x86.Registers.EDX
          };
        // move low result to left low
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ESP,
            DestinationIsIndirect = true,
            DestinationDisplacement = 8,
            SourceReg = Cosmos.Assembler.x86.Registers.EAX
          };
        // pop right 64 value
        new Assembler.x86.Add { DestinationReg = Cosmos.Assembler.x86.Registers.ESP, SourceValue = 8 };
      }
      else
      {
        new Assembler.x86.Pop { DestinationReg = Cosmos.Assembler.x86.Registers.EAX };
        new Assembler.x86.Multiply
          {
            DestinationReg = Cosmos.Assembler.x86.Registers.ESP,
            DestinationIsIndirect = true,
            Size = 32
          };
        new Assembler.x86.Add { DestinationReg = Cosmos.Assembler.x86.Registers.ESP, SourceValue = 4 };
        new Assembler.x86.Push { DestinationReg = Cosmos.Assembler.x86.Registers.EAX };
      }
    }
  }
}
