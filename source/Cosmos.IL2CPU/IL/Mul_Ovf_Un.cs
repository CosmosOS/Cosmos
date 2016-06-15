using System;

using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using XSharp.Compiler;

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
            DestinationReg = Cosmos.Assembler.x86.RegistersEnum.EAX,
            SourceReg = Cosmos.Assembler.x86.RegistersEnum.ESP,
            SourceIsIndirect = true,
            SourceDisplacement = 4
          };
        new Assembler.x86.Or
          {
            DestinationReg = Cosmos.Assembler.x86.RegistersEnum.EAX,
            SourceReg = Cosmos.Assembler.x86.RegistersEnum.ESP,
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
            DestinationReg = Cosmos.Assembler.x86.RegistersEnum.EAX,
            SourceReg = Cosmos.Assembler.x86.RegistersEnum.ESP,
            SourceIsIndirect = true,
            SourceDisplacement = 4
          };
        // eax contains already RIGHT_HIGH
        // multiply with LEFT_LOW
        XS.Multiply(XSRegisters.OldToNewRegister(RegistersEnum.ESP), displacement: 8);
        // save result of LEFT_LOW * RIGHT_HIGH
        XS.Set(XSRegisters.OldToNewRegister(RegistersEnum.ECX), XSRegisters.OldToNewRegister(RegistersEnum.EAX));

        //mov RIGHT_LOW to eax
        new Mov
          {
            DestinationReg = RegistersEnum.EAX,
            SourceReg = RegistersEnum.ESP,
            SourceIsIndirect = true
          };
        // multiply with LEFT_HIGH
        XS.Multiply(XSRegisters.OldToNewRegister(RegistersEnum.ESP), displacement: 12);
        // add result of LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH
        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ECX), XSRegisters.OldToNewRegister(RegistersEnum.EAX));

        //mov RIGHT_LOW to eax
        new Mov
          {
            DestinationReg = RegistersEnum.EAX,
            SourceReg = RegistersEnum.ESP,
            SourceIsIndirect = true
          };
        // multiply with LEFT_LOW
        XS.Multiply(XSRegisters.OldToNewRegister(RegistersEnum.ESP), displacement: 8);
        // add LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH to high dword of last result
        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.EDX), XSRegisters.OldToNewRegister(RegistersEnum.ECX));

        new Jump { DestinationLabel = MoveReturnValue };

        new Label(Simple32Multiply);
        //mov RIGHT_LOW to eax
        new Mov
          {
            DestinationReg = RegistersEnum.EAX,
            SourceReg = RegistersEnum.ESP,
            SourceIsIndirect = true
          };
        // multiply with LEFT_LOW
        XS.Multiply(XSRegisters.OldToNewRegister(RegistersEnum.ESP), displacement: 8);

        new Label(MoveReturnValue);
        // move high result to left high
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.RegistersEnum.ESP,
            DestinationIsIndirect = true,
            DestinationDisplacement = 12,
            SourceReg = Cosmos.Assembler.x86.RegistersEnum.EDX
          };
        // move low result to left low
        new Assembler.x86.Mov
          {
            DestinationReg = Cosmos.Assembler.x86.RegistersEnum.ESP,
            DestinationIsIndirect = true,
            DestinationDisplacement = 8,
            SourceReg = Cosmos.Assembler.x86.RegistersEnum.EAX
          };
        // pop right 64 value
        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 8);
      }
      else
      {
        XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
        new Multiply
          {
            DestinationReg = RegistersEnum.ESP,
            DestinationIsIndirect = true,
            Size = 32
          };
        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
        XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
      }
    }
  }
}
