using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs.System;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Box)]
  public class Box : ILOp
  {
    public Box(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      OpType xType = (OpType)aOpCode;

      uint xSize = Align(SizeOfType(xType.Value), 4);
      string xTypeID = GetTypeIDLabel(xType.Value);
      new CPUx86.Push { DestinationValue = (ObjectImpl.FieldDataOffset + xSize) };
      new CPUx86.Call { DestinationLabel = LabelName.Get(GCImplementationRefs.AllocNewObjectRef) };
      new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EAX };
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.EAX, SourceIsIndirect = true };
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EBX, SourceRef = Cosmos.Assembler.ElementReference.New(xTypeID), SourceIsIndirect = true };
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EBX };
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.BoxedValueType, Size = 32 };
      new Comment(Assembler, "xSize is " + xSize);
      for (int i = 0; i < (xSize / 4); i++)
      {
        new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EDX };
        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, DestinationIsIndirect = true, DestinationDisplacement = (ObjectImpl.FieldDataOffset + (i * 4)), SourceReg = CPUx86.RegistersEnum.EDX, Size = 32 };
      }
      new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EAX };
    }
  }
}
