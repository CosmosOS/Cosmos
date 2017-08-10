using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldind_U1)]
  public class Ldind_U1 : ILOp
  {
    public Ldind_U1(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      DoNullReferenceCheck(Assembler, DebugEnabled, 0);
      XS.Pop(XSRegisters.ECX);
      XS.MoveZeroExtend(EAX, ECX, sourceIsIndirect: true, size: RegisterSize.Byte8);
      XS.Push(XSRegisters.EAX);
    }
  }
}
