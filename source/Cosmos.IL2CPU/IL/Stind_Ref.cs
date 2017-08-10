using System;

using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stind_Ref)]
  public class Stind_Ref : ILOp
  {
    public Stind_Ref(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      Stind_I.Assemble(Assembler, 8, DebugEnabled);
    }
  }
}
