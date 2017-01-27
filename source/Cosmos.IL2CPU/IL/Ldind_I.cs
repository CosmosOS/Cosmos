using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldind_I)]
  public class Ldind_I : ILOp
  {
    public Ldind_I(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      Assemble(Assembler, 4, DebugEnabled);
    }

    public static void Assemble(Cosmos.Assembler.Assembler aAssembler, int aSize, bool debugEnabled)
    {
      int xAlignedSize = (int)Align((uint)aSize, 4);

      XS.Comment("address at: [esp]");
      DoNullReferenceCheck(aAssembler, debugEnabled, 0);

      XS.Pop(EAX);

      for (int i = xAlignedSize - 4; i >= 0; i -= 4)
      {
        XS.Push(EAX, isIndirect: true, displacement: i);
      }
    }
  }
}
