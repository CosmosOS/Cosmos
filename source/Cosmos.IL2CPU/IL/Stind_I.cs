using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stind_I)]
  public class Stind_I : ILOp
  {
    public Stind_I(Cosmos.Assembler.Assembler aAsmblr)
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

      XS.Comment($"address at: [esp+{xAlignedSize}]");
      DoNullReferenceCheck(aAssembler, debugEnabled, xAlignedSize);

      XS.Set(EAX, ESP, sourceIsIndirect: true, sourceDisplacement: xAlignedSize);
      for (int i = 0; i < xAlignedSize; i += 4)
      {
        XS.Pop(EBX);
        XS.Set(EAX, EBX, destinationIsIndirect: true, destinationDisplacement: i);
      }
      XS.Add(ESP, 4);
    }
  }
}
