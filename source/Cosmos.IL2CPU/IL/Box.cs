using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Box)]
  public class Box : ILOp
  {
    public Box(Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      OpType xType = (OpType)aOpCode;
      uint xSize = Align(SizeOfType(xType.Value), 4);
      string xTypeID = GetTypeIDLabel(xType.Value);

      XS.Push(ObjectImpl.FieldDataOffset + xSize);
      XS.Call(LabelName.Get(GCImplementationRefs.AllocNewObjectRef));
      XS.Pop(ESI);
      XS.Set(EBX, xTypeID, sourceIsIndirect: true);
      XS.Set(ESI, EBX, destinationIsIndirect: true);
      XS.Set(ESI, (uint)InstanceTypeEnum.BoxedValueType, destinationDisplacement: 4, size: RegisterSize.Int32);
      new Comment(Assembler, "xSize is " + xSize);
      for (int i = 0; i < (xSize / 4); i++)
      {
        XS.Pop(EDX);
        XS.Set(ESI, EDX, destinationDisplacement: (ObjectImpl.FieldDataOffset + (i * 4)), size: RegisterSize.Int32);
      }
      XS.Push(ESI);
      XS.Push(0);
    }
  }
}
