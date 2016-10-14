using System;
using System.Drawing;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Ldloc)]
  public class Ldloc : ILOp
  {
    public Ldloc(Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar)aOpCode;
      var xVar = aMethod.MethodBase.GetMethodBody().LocalVariables[xOpVar.Value];
      var xStackCount = (int)GetStackCountForLocal(aMethod, xVar);
      var xEBPOffset = (int)GetEBPOffsetForLocal(aMethod, xOpVar.Value);
      var xSize = SizeOfType(xVar.LocalType);

      XS.Comment("Local type = " + xVar.LocalType);
      XS.Comment("Local EBP offset = " + xEBPOffset);
      XS.Comment("Local size = " + xSize);

      switch (xSize)
      {
        case 1:
        case 2:
          bool xSigned = IsIntegerSigned(xVar.LocalType);
          if (xSigned)
          {
            new CPUx86.MoveSignExtend { DestinationReg = CPUx86.RegistersEnum.EAX, Size = (byte)(xSize * 8), SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = (int)(0 - xEBPOffset) };
          }
          else
          {
            new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.RegistersEnum.EAX, Size = (byte)(xSize * 8), SourceReg = CPUx86.RegistersEnum.EBP, SourceIsIndirect = true, SourceDisplacement = (int)(0 - xEBPOffset) };
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
