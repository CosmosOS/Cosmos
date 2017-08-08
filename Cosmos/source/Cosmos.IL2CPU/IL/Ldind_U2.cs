using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldind_U2)]
  public class Ldind_U2 : ILOp
  {
    public Ldind_U2(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      DoNullReferenceCheck(Assembler, DebugEnabled, 0);
      XS.Pop(ECX);
      XS.MoveZeroExtend(EAX, ECX, sourceIsIndirect: true, size: RegisterSize.Short16);
      XS.Push(EAX);
    }
  }
}
