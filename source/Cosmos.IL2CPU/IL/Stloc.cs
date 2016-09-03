using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Stloc)]
  public class Stloc : ILOp
  {
    public Stloc(Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar) aOpCode;
      var xVar = aMethod.MethodBase.GetMethodBody().LocalVariables[xOpVar.Value];
      var xStackCount = (int) GetStackCountForLocal(aMethod, xVar);
      var xEBPOffset = (int) GetEBPOffsetForLocal(aMethod, xOpVar.Value);
      var xSize = SizeOfType(xVar.LocalType);
      XS.Comment(xVar.LocalType.ToString());
      XS.Comment("EBPOffset = " + xEBPOffset);
      XS.Comment("xSize = " + xSize);

      for (int i = xStackCount - 1; i >= 0; i--)
      {
        XS.Pop(EAX);
        XS.Set(EBP, EAX, destinationDisplacement: 0 - (xEBPOffset + (i*4)));
      }
    }
  }
}
