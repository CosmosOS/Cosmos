using Cosmos.Debug.Symbols;

using Cosmos.IL2CPU.Extensions;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Stloc)]
  public class Stloc : ILOp
  {
    public Stloc(Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar) aOpCode;
      var xVar = DebugSymbolReader.GetLocalVariableInfos(aMethod.MethodBase)[xOpVar.Value];
      var xStackCount = (int) GetStackCountForLocal(aMethod, xVar.Type);
      var xEBPOffset = (int) GetEBPOffsetForLocal(aMethod, xOpVar.Value);
      var xSize = SizeOfType(xVar.Type);

      XS.Comment("Local type = " + xVar);
      XS.Comment("Local EBP offset = " + xEBPOffset);
      XS.Comment("Local size = " + xSize);

      for (int i = xStackCount - 1; i >= 0; i--)
      {
        XS.Pop(EAX);
        XS.Set(EBP, EAX, destinationDisplacement: 0 - (xEBPOffset + (i*4)));
      }
    }
  }
}
