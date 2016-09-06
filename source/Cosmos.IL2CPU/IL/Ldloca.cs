using System;
using System.Runtime.InteropServices;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Ldloca)]
  public class Ldloca : ILOp
  {
    public Ldloca(Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar)aOpCode;
      var xVar = aMethod.MethodBase.GetMethodBody().LocalVariables[xOpVar.Value];
      var xStackCount = (int)GetStackCountForLocal(aMethod, xVar);
      var xEBPOffset = (int)GetEBPOffsetForLocal(aMethod, xOpVar.Value);
      xEBPOffset += (xStackCount - 1)*4;

      XS.Comment("Local type = " + xVar.LocalType);
      XS.Comment("Local EBP offset = " + xEBPOffset);

      XS.Set(EAX, EBP);
      XS.Set(EBX, (uint)(xEBPOffset));
      XS.Sub(EAX, EBX);
      XS.Push(EAX);
    }
  }
}
