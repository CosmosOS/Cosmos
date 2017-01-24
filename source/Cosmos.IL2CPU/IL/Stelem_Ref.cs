using System;

using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stelem_Ref)]
  public class Stelem_Ref : ILOp
  {
    public Stelem_Ref(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Assemble(Assembler, 8, aMethod, aOpCode, DebugEnabled);
    }

    public static void Assemble(Assembler.Assembler aAssembler, uint aElementSize, MethodInfo aMethod, ILOpCode aOpCode, bool debugEnabled)
    {
      // stack     == the new value
      // stack + 1 == the index
      // stack + 3 == the array
      DoNullReferenceCheck(aAssembler, debugEnabled, (int)(8 + Align(aElementSize, 4)));

      uint xStackSize = aElementSize;
      if (xStackSize % 4 != 0)
      {
        xStackSize += 4 - xStackSize % 4;
      }

      // calculate element offset into array memory (including header)
      XS.Set(EAX, ESP, sourceDisplacement: (int)xStackSize); // the index
      XS.Set(EDX, aElementSize);
      XS.Multiply(EDX);
      XS.Add(EAX, ObjectImpl.FieldDataOffset + 4);

      XS.Set(EDX, ESP, sourceDisplacement: (int)xStackSize + 8); // the array
      XS.Add(EDX, EAX);
      XS.Push(EDX);

      XS.Pop(ECX);

      for (int i = (int)(aElementSize / 4) - 1; i >= 0; i -= 1)
      {
        XS.Comment("Start 1 dword");
        XS.Pop(EBX);
        XS.Set(ECX, EBX, destinationIsIndirect: true);
        XS.Add(ECX, 4);
      }

      switch (aElementSize % 4)
      {
        case 1:
          {
            XS.Comment("Start 1 byte");
            XS.Pop(EBX);
            XS.Set(ECX, BL, destinationIsIndirect: true);
            break;
          }
        case 2:
          {
            XS.Comment("Start 1 word");
            XS.Pop(EBX);
            XS.Set(ECX, BX, destinationIsIndirect: true);
            break;
          }
        case 0:
          {
            break;
          }
        default:
          throw new NotImplementedException("Remainder size " + (aElementSize % 4) + " not supported!");

      }
      XS.Add(ESP, 12);
    }
  }
}
