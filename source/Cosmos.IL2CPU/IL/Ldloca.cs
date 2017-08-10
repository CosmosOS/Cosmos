using Cosmos.Debug.Symbols;

using Cosmos.IL2CPU.Extensions;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Ldloca)]
  public class Ldloca : ILOp
  {
    public Ldloca(Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar)aOpCode;
      var xVar = DebugSymbolReader.GetLocalVariableInfos(aMethod.MethodBase)[xOpVar.Value];
      var xEBPOffset = GetEBPOffsetForLocal(aMethod, xOpVar.Value);
      xEBPOffset += (uint)(((int)GetStackCountForLocal(aMethod, xVar.Type) - 1) * 4);

      XS.Comment("Local type = " + xVar);
      XS.Comment("Local EBP offset = " + xEBPOffset);

      XS.Set(EAX, EBP);
      XS.Sub(EAX, xEBPOffset);
      XS.Push(EAX);
    }
  }
}
