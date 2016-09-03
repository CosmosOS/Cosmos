using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stelem_Ref)]
  public class Stelem_Ref : ILOp
  {
    public Stelem_Ref(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public static void Assemble(Assembler.Assembler aAssembler, uint aElementSize, MethodInfo aMethod, ILOpCode aOpCode, bool debugEnabled)
    {
      // stack     == the new value
      // stack + 1 == the index
      // stack + 2 == the array
      DoNullReferenceCheck(aAssembler, debugEnabled, (int)(8 + Align(aElementSize, 4)));

      uint xStackSize = aElementSize;
      if (xStackSize % 4 != 0)
      {
        xStackSize += 4 - xStackSize % 4;
      }

      // calculate element offset into array memory (including header)
      XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceDisplacement: (int)xStackSize); // the index
      XS.Set(XSRegisters.EDX, aElementSize);
      XS.Multiply(XSRegisters.EDX);
      XS.Add(XSRegisters.EAX, ObjectImpl.FieldDataOffset + 4);

      XS.Set(XSRegisters.EDX, XSRegisters.ESP, sourceDisplacement: (int)xStackSize + 8); // the array
      XS.Add(XSRegisters.EDX, XSRegisters.EAX);
      XS.Push(XSRegisters.EDX);

      XS.Pop(XSRegisters.ECX);
      for (int i = (int)(aElementSize / 4) - 1; i >= 0; i -= 1)
      {
        new Comment(aAssembler, "Start 1 dword");
        XS.Pop(XSRegisters.EBX);
        XS.Set(XSRegisters.ECX, XSRegisters.EBX, destinationIsIndirect: true);
        XS.Add(XSRegisters.ECX, 4);
      }
      switch (aElementSize % 4)
      {
        case 1:
          {
            new Comment(aAssembler, "Start 1 byte");
            XS.Pop(XSRegisters.EBX);
            XS.Set(XSRegisters.ECX, XSRegisters.BL, destinationIsIndirect: true);
            break;
          }
        case 2:
          {
            new Comment(aAssembler, "Start 1 word");
            XS.Pop(XSRegisters.EBX);
            XS.Set(XSRegisters.ECX, XSRegisters.BX, destinationIsIndirect: true);
            break;
          }
        case 0:
          {
            break;
          }
        default:
          throw new Exception("Remainder size " + (aElementSize % 4) + " not supported!");

      }
      XS.Add(XSRegisters.ESP, 12);
    }
    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Assemble(Assembler, 8, aMethod, aOpCode, DebugEnabled);
    }
  }
}
