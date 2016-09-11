using System;
using System.Reflection;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Constrained)]
  public class Constrained : ILOp
  {
    public Constrained(Assembler.Assembler aAsmblr) : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpType = aOpCode as OpType;
      DoExecute(Assembler, aMethod, aOpCode, xOpType, DebugEnabled);
    }

    private void DoExecute(Assembler.Assembler assembler, MethodInfo aMethod, ILOpCode aOpCode, OpType aTargetType, bool debugEnabled)
    {
      new Comment(assembler, $"Type = {aTargetType.Value}");
      if (aTargetType.Value.BaseType == typeof(ValueType))
      {

      }
      else if (aTargetType.Value.BaseType == typeof(object))
      {
        throw new NotImplementedException($"Constrained not implemented for {aTargetType.Value}");
      }
    }
  }
}
