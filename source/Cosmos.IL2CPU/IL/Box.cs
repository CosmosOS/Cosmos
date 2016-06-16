using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;

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
      XS.Call(LabelName.Get(GCImplementationRefs.AllocNewObjectRef));
      XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
      XS.Set(XSRegisters.ESI, XSRegisters.EAX, sourceIsIndirect: true);
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EBX, SourceRef = ElementReference.New(xTypeID), SourceIsIndirect = true };
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EBX };
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.BoxedValueType, Size = 32 };
      new Comment(Assembler, "xSize is " + xSize);
      for (int i = 0; i < (xSize / 4); i++)
      {
        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, DestinationIsIndirect = true, DestinationDisplacement = (ObjectImpl.FieldDataOffset + (i * 4)), SourceReg = CPUx86.RegistersEnum.EDX, Size = 32 };
      }
      XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
    }
  }
}
