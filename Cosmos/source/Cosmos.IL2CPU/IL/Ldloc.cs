using Cosmos.Debug.Symbols;

using Cosmos.IL2CPU.Extensions;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Common;
using CPUx86 = Cosmos.Assembler.x86;
using static XSharp.Common.XSRegisters;


namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Ldloc)]
  public class Ldloc : ILOp
  {
    public Ldloc(Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar)aOpCode;
      var xVar = DebugSymbolReader.GetLocalVariableInfos(aMethod.MethodBase)[xOpVar.Value];
      var xStackCount = (int)GetStackCountForLocal(aMethod, xVar.Type);
      var xEBPOffset = (int)GetEBPOffsetForLocal(aMethod, xOpVar.Value);
      var xSize = SizeOfType(xVar.Type);
      bool xSigned = IsIntegerSigned(xVar.Type);

      XS.Comment("Local type = " + xVar);
      XS.Comment("Local EBP offset = " + xEBPOffset);
      XS.Comment("Local size = " + xSize);

      switch (xSize)
      {
        case 1:
          if (xSigned)
          {
            XS.MoveSignExtend(EAX, EBP, sourceIsIndirect: true, sourceDisplacement: (0 - xEBPOffset), size: RegisterSize.Byte8);
          }
          else
          {
            XS.MoveZeroExtend(EAX, EBP, sourceIsIndirect: true, sourceDisplacement: (0 - xEBPOffset), size: RegisterSize.Byte8);
          }
          XS.Push(EAX);
          break;
        case 2:
          if (xSigned)
          {
            XS.MoveSignExtend(EAX, EBP, sourceIsIndirect: true, sourceDisplacement: (0 - xEBPOffset), size: RegisterSize.Short16);
          }
          else
          {
            XS.MoveZeroExtend(EAX, EBP, sourceIsIndirect: true, sourceDisplacement: (0 - xEBPOffset), size: RegisterSize.Short16);
          }
          XS.Push(EAX);
          break;
        default:
          for (int i = 0; i < xStackCount; i++)
          {
            XS.Set(EAX, EBP, sourceDisplacement: 0 - (xEBPOffset + (i * 4)));
            XS.Push(EAX);
          }
          break;
      }
    }
  }
}
