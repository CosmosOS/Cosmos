using System;

using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_Ovf_I4)]
  public class Conv_Ovf_I4 : ILOp
  {
    public Conv_Ovf_I4(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      ThrowNotImplementedException("Conv_Ovf_I4 is not yet implemented");
    }
  }
}
