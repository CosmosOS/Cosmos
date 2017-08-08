using System.Reflection;

using Cosmos.IL2CPU.ILOpCodes;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldelem)]
  public class Ldelem : ILOp
  {
    public Ldelem(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpType = (OpType) aOpCode;
      var xSize = SizeOfType(xOpType.Value);
      if (xOpType.Value.GetTypeInfo().IsValueType && !xOpType.Value.GetTypeInfo().IsPrimitive)
      {
        Ldelema.Assemble(Assembler, xOpType, xSize, DebugEnabled);
        Ldobj.DoAssemble(xOpType.Value);
        return;
      }
      Ldelem_Ref.Assemble(Assembler, xSize, false, aMethod, aOpCode, DebugEnabled);
    }
  }
}