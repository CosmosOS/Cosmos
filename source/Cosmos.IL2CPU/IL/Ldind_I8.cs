using System;

using XSharp.Common;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldind_I8)]
  public class Ldind_I8 : ILOp
  {
    public Ldind_I8(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      DoNullReferenceCheck(Assembler, DebugEnabled, 0);
      XS.Pop(XSRegisters.EAX);
      XS.Push(XSRegisters.EAX, isIndirect: true, displacement: 4);
      XS.Push(XSRegisters.EAX, isIndirect: true);
    }
  }
}
