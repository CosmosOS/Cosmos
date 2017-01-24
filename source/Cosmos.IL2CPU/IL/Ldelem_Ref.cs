using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

using ObjectInfo = Cosmos.IL2CPU.Plugs.System.ObjectImpl;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Ldelem_Ref)]
  public class Ldelem_Ref : ILOp
  {
    public Ldelem_Ref(Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Assemble(Assembler, 8, false, aMethod, aOpCode, DebugEnabled);
    }

    public static void Assemble(Assembler.Assembler aAssembler, uint aElementSize, bool isSigned, MethodInfo aMethod, ILOpCode aOpCode, bool debugEnabled)
    {
      //  stack     = index
      //  stack + 2 = array
      DoNullReferenceCheck(aAssembler, debugEnabled, 8);

      // calculate element offset into array memory (including header)
      XS.Pop(EAX);
      XS.Set(EDX, aElementSize);
      XS.Multiply(EDX);
      XS.Add(EAX, ObjectInfo.FieldDataOffset + 4);

      if (aElementSize > 4)
      {
        // we start copying the last bytes
        XS.Add(EAX, aElementSize - 4);
      }

      // pop the array now
      XS.Add(ESP, 4);
      XS.Pop(EDX);

      XS.Add(EDX, EAX);

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
              XS.MoveSignExtend(ECX, EDX, sourceIsIndirect: true, size: (RegisterSize)8);
            }
            else
            {
              XS.MoveZeroExtend(ECX, EDX, sourceIsIndirect: true, size: (RegisterSize)8);
            }
            XS.Push(ECX);
            break;
          case 2:
            if (isSigned)
            {
              XS.MoveSignExtend(ECX, EDX, sourceIsIndirect: true, size: (RegisterSize)16);
            }
            else
            {
              XS.MoveZeroExtend(ECX, EDX, sourceIsIndirect: true, size: (RegisterSize)16);
            }
            XS.Push(ECX);
            break;
          case 4:
            // copy a full dword
            XS.Push(EDX, isIndirect: true);
            XS.Sub(EDX, 4); // move to previous 4 bytes
            break;
            //case 8:
            //    new CPUx86.Push {DestinationReg = CPUx86.Registers.EDX, DestinationDisplacement = 4, DestinationIsIndirect = true};
            //    XS.Push(XSRegisters.EDX, isIndirect: true);
            //    break;
        }
      }
    }
  }
}
