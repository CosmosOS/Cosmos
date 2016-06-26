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

    public static void Assemble(Cosmos.Assembler.Assembler aAssembler, uint aElementSize, MethodInfo aMethod, ILOpCode aOpCode, bool debugEnabled)
    {
      DoNullReferenceCheck(aAssembler, debugEnabled, 4 + Align(aElementSize, 4));
      // stack - 3 == the array
      // stack - 2 == the index
      // stack - 1 == the new value
      uint xStackSize = aElementSize;
      if (xStackSize % 4 != 0)
      {
        xStackSize += 4 - xStackSize % 4;
      }
      //new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xStackSize + 4) };

      //XS.Call(MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef));

      XS.Set(XSRegisters.EBX, XSRegisters.ESP, sourceDisplacement: (int)xStackSize); // the index
      XS.Set(XSRegisters.ECX, XSRegisters.ESP, sourceDisplacement: (int)xStackSize + 4); // the array
      // now convert the array handle to an actual memory address
      XS.Set(XSRegisters.ECX, XSRegisters.ECX, sourceIsIndirect: true);

      XS.Add(XSRegisters.ECX, (uint)(ObjectImpl.FieldDataOffset + 4));

      XS.Push(aElementSize);
      XS.Push(XSRegisters.EBX);


      //Multiply( aAssembler, aServiceProvider, aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
      string xBaseLabel = GetLabel(aMethod, aOpCode) + ".";

      Mul.DoExecute(4, false, xBaseLabel);

      XS.Push(XSRegisters.ECX);

      //Add( aAssembler, aServiceProvider, aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
      Add.DoExecute(4, false);

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
      XS.Add(XSRegisters.ESP, 0x8);
    }
    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Assemble(Assembler, 4, aMethod, aOpCode, DebugEnabled);
    }
  }
}
