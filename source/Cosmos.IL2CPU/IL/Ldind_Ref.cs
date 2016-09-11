using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Ldind_Ref)]
  public class Ldind_Ref : ILOp
  {
    public Ldind_Ref(Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Ldind_I.Assemble(Assembler, 8, DebugEnabled);
    }
  }
}
