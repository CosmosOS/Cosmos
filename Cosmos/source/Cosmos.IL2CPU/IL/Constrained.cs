using System;
using System.Reflection;

using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Common;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Constrained)]
  public class Constrained : ILOp
  {
    public Constrained(Assembler.Assembler aAsmblr) : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpType = aOpCode as OpType;
      DoExecute(Assembler, aMethod, aOpCode, xOpType, DebugEnabled);
    }

    private void DoExecute(Assembler.Assembler assembler, _MethodInfo aMethod, ILOpCode aOpCode, OpType aTargetType, bool debugEnabled)
    {
      var xType = aTargetType.Value;

      XS.Comment($"Type = {aTargetType.Value}");
      if (xType.GetTypeInfo().BaseType == typeof(ValueType))
      {

      }
      else if (xType.GetTypeInfo().BaseType == typeof(object))
      {
        throw new NotImplementedException($"Constrained not implemented for {aTargetType.Value}");
      }
    }
  }
}
