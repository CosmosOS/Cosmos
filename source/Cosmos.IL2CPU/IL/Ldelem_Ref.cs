using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldelem_Ref)]
  public class Ldelem_Ref : ILOp
  {
    public Ldelem_Ref(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public static void Assemble(Cosmos.Assembler.Assembler aAssembler, uint aElementSize, bool isSigned, bool debugEnabled)
    {
      // stack - 1: array
      // stack - 0: index
      DoNullReferenceCheck(aAssembler, debugEnabled, 4);

      XS.Pop(XSRegisters.EAX); // index
      XS.Set(XSRegisters.EDX, aElementSize);
      XS.Multiply(XSRegisters.EDX);

      XS.Add(XSRegisters.EAX, (ObjectImpl.FieldDataOffset + 4));

      if (aElementSize > 4)
      {
        // we start copying the last bytes
        XS.Add(XSRegisters.EAX, aElementSize - 4);
      }

      // pop the array
      XS.Pop(XSRegisters.EDX);
      XS.Add(XSRegisters.ESP, 4);

      XS.Add(XSRegisters.EDX, XSRegisters.EAX);

      var xSizeLeft = aElementSize;
      while (xSizeLeft > 0)
      {
        var xCurrentStep = Math.Min(xSizeLeft, 4);
        if (xSizeLeft % 4 != 0)
        {
          xCurrentStep = xSizeLeft % 4;
        }
        xSizeLeft = xSizeLeft - xCurrentStep;
        switch (xCurrentStep)
        {
          case 1:
            if (isSigned)
            {
              new CPUx86.MoveSignExtend { DestinationReg = CPUx86.RegistersEnum.ECX, Size = 8, SourceReg = CPUx86.RegistersEnum.EDX, SourceIsIndirect = true };
            }
            else
            {
              new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.RegistersEnum.ECX, Size = 8, SourceReg = CPUx86.RegistersEnum.EDX, SourceIsIndirect = true };
            }
            XS.Push(XSRegisters.ECX);
            break;
          case 2:
            if (isSigned)
            {
              new CPUx86.MoveSignExtend { DestinationReg = CPUx86.RegistersEnum.ECX, Size = 16, SourceReg = CPUx86.RegistersEnum.EDX, SourceIsIndirect = true };
            }
            else
            {
              new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.RegistersEnum.ECX, Size = 16, SourceReg = CPUx86.RegistersEnum.EDX, SourceIsIndirect = true };
            }
            XS.Push(XSRegisters.ECX);
            break;
          case 4:
            // copy a full dword
            XS.Push(XSRegisters.EDX, isIndirect: true);
            XS.Sub(XSRegisters.EDX, 4); // move to previous 4 bytes
            break;
            //case 8:
            //    new CPUx86.Push {DestinationReg = CPUx86.Registers.EDX, DestinationDisplacement = 4, DestinationIsIndirect = true};
            //    XS.Push(XSRegisters.EDX, isIndirect: true);
            //    break;
        }
      }
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Assemble(Assembler, 4, false, DebugEnabled);
    }
  }
}
