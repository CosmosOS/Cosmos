using System;

using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Mul_Ovf)]
  public class Mul_Ovf : ILOp
  {
    public Mul_Ovf(Cosmos.Assembler.Assembler aAsmblr)
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
        XS.Set(XSRegisters.OldToNewRegister(RegistersEnum.EAX), XSRegisters.OldToNewRegister(RegistersEnum.ESP), sourceDisplacement: 4);
        XS.Or(XSRegisters.OldToNewRegister(RegistersEnum.EAX), XSRegisters.OldToNewRegister(RegistersEnum.ESP), sourceDisplacement: 12);
        XS.Jump(ConditionalTestEnum.Zero, Simple32Multiply);
        // Full 64 Multiply

        // copy again, or could change EAX
        //TODO is there an opcode that does OR without change EAX?
        XS.Set(XSRegisters.OldToNewRegister(RegistersEnum.EAX), XSRegisters.OldToNewRegister(RegistersEnum.ESP), sourceDisplacement: 4);
        // eax contains already RIGHT_HIGH
        // multiply with LEFT_LOW
        XS.Multiply(XSRegisters.OldToNewRegister(RegistersEnum.ESP), displacement: 8);
        // save result of LEFT_LOW * RIGHT_HIGH
        XS.Set(XSRegisters.OldToNewRegister(RegistersEnum.ECX), XSRegisters.OldToNewRegister(RegistersEnum.EAX));

        //mov RIGHT_LOW to eax
        XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
        // multiply with LEFT_HIGH
        XS.Multiply(XSRegisters.OldToNewRegister(RegistersEnum.ESP), displacement: 12);
        // add result of LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH
        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ECX), XSRegisters.OldToNewRegister(RegistersEnum.EAX));

        //mov RIGHT_LOW to eax
        XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
        // multiply with LEFT_LOW
        XS.Multiply(XSRegisters.OldToNewRegister(RegistersEnum.ESP), displacement: 8);
        // add LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH to high dword of last result
        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.EDX), XSRegisters.OldToNewRegister(RegistersEnum.ECX));

        XS.Jump(MoveReturnValue);

        XS.Label(Simple32Multiply);
        //mov RIGHT_LOW to eax
        XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
        // multiply with LEFT_LOW
        XS.Multiply(XSRegisters.OldToNewRegister(RegistersEnum.ESP), displacement: 8);

        XS.Label(MoveReturnValue);
        // move high result to left high
        XS.Set(XSRegisters.ESP, XSRegisters.EDX, destinationDisplacement: 12);
        // move low result to left low
        XS.Set(XSRegisters.ESP, XSRegisters.EAX, destinationDisplacement: 8);
        // pop right 64 value
        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 8);
      }
      else
      {
        XS.Pop(XSRegisters.EAX);
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
