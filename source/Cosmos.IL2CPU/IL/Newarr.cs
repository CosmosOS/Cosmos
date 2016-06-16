using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using System.Reflection;

using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
  /// <summary>
  /// Pushes an object reference to a new zero-based, one-dimensional array whose elements are of a specific type onto the evaluation stack.
  /// </summary>
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Newarr)]
  public class Newarr : ILOp
  {
    public Newarr(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Cosmos.IL2CPU.ILOpCodes.OpType xType = (Cosmos.IL2CPU.ILOpCodes.OpType)aOpCode;

      uint xSize = SizeOfType(xType.Value);

      //TODO cache it to reduce calculation
      string xTypeID = GetTypeIDLabel(typeof(Array));
      MethodBase xCtor = typeof(Array).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)[0];
      string xCtorName = LabelName.Get(xCtor);

      new Comment(Assembler, "Element Size = " + xSize);
      // element count is on the stack
      XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
      XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
      //Assembler.StackSizes.Push(xElementCountSize);
      new CPUx86.Push { DestinationValue = xSize };
      new Mul(Assembler).Execute(aMethod, aOpCode);
      // the total items size is now on the stack
      new CPUx86.Push { DestinationValue = (ObjectImpl.FieldDataOffset + 4) };
      new Add(Assembler).Execute(aMethod, aOpCode);
      // the total array size is now on the stack.
      XS.Call(LabelName.Get(GCImplementationRefs.AllocNewObjectRef));
      new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
      new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
      // it's on the stack 3 times now, once from the return value, twice from the pushes;

      XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
      XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EBX, SourceRef = Cosmos.Assembler.ElementReference.New(xTypeID), SourceIsIndirect = true };
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EBX };
      XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 4);
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true, SourceValue = (uint)InstanceTypeEnum.Array, Size = 32 };
      XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 4);
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.ESI, Size = 32 };
      XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 4);
      new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true, SourceValue = (uint)xSize, Size = 32 };
      XS.Call(xCtorName);
    }
  }
}
