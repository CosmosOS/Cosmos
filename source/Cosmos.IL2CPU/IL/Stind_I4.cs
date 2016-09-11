using System;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stind_I4)]
  public class Stind_I4 : ILOp
  {
    public Stind_I4(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Stind_I.Assemble(Assembler, 4, DebugEnabled);
    }
  }
}
