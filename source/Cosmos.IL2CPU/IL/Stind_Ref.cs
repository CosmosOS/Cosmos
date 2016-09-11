using System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stind_Ref)]
  public class Stind_Ref : ILOp
  {
    public Stind_Ref(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Stind_I.Assemble(Assembler, 8, DebugEnabled);
    }
  }
}
