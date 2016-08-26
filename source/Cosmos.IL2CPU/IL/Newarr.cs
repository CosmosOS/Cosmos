using System;
using Cosmos.Assembler;
using System.Reflection;

using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using ObjectInfo = Cosmos.IL2CPU.Plugs.System.ObjectImpl;

namespace Cosmos.IL2CPU.X86.IL
{
  /// <summary>
  /// Pushes an object reference to a new zero-based, one-dimensional array whose elements are of a specific type onto the evaluation stack.
  /// </summary>
  [OpCode(ILOpCode.Code.Newarr)]
  public class Newarr : ILOp
  {
    public Newarr(Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      ILOpCodes.OpType xType = (ILOpCodes.OpType)aOpCode;

      uint xSize = SizeOfType(xType.Value);

      //TODO cache it to reduce calculation
      string xTypeID = GetTypeIDLabel(typeof(Array));
      MethodBase xCtor =
          typeof(Array).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static |
                                        BindingFlags.Instance)[0];
      string xCtorName = LabelName.Get(xCtor);

      XS.Comment("Element Size = " + xSize);
      // element count is on the stack
      XS.Pop(ESI);
      XS.Push(ESI);
      XS.Push(xSize);
      new Mul(Assembler).Execute(aMethod, aOpCode);
      // the total items size is now on the stack
      XS.Push(ObjectInfo.FieldDataOffset + 4);
      new Add(Assembler).Execute(aMethod, aOpCode);
      // the total array size is now on the stack.
      XS.Call(LabelName.Get(GCImplementationRefs.AllocNewObjectRef));
      XS.Label(".AfterAlloc");
      XS.Push(ESP, isIndirect: true);
      XS.Push(ESP, isIndirect: true);

      // it's on the stack 3 times now, once from the return value, twice from the pushes;
      XS.Pop(EAX);
      XS.Set(EBX, xTypeID, sourceIsIndirect: true);
      XS.Set(EAX, EBX, destinationIsIndirect: true);
      XS.Set(EAX, (uint)InstanceTypeEnum.Array, destinationDisplacement: 4);
      XS.Set(EAX, ESI, destinationDisplacement: 8);
      XS.Set(EAX, xSize, destinationDisplacement: 12);
      XS.Push(0);
      XS.Call(xCtorName);
      XS.Pop(EAX);
      XS.Push(EAX);
      XS.Push(0);
    }
  }
}
