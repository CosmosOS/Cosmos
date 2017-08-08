using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using System.Reflection;
using System.Linq;

using XSharp.Common;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldsflda)]
  public class Ldsflda : ILOp
  {
    public Ldsflda(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpCode = (ILOpCodes.OpField) aOpCode;
      var xFieldName = DataMember.GetStaticFieldName(xOpCode.Value);
      DoExecute(Assembler, aMethod, xFieldName, xOpCode.Value.DeclaringType, aOpCode);
    }

    public static void DoExecute(Cosmos.Assembler.Assembler assembler, _MethodInfo aMethod, string field, Type declaringType, ILOpCode aCurrentOpCode)
    {
      // call cctor:
      var xCctor = (declaringType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic) ?? new ConstructorInfo[0]).SingleOrDefault();
      if (xCctor != null)
      {
        XS.Call(LabelName.Get(xCctor));
        if (aCurrentOpCode != null)
        {
          ILOp.EmitExceptionLogic(assembler, aMethod, aCurrentOpCode, true, null, ".AfterCCTorExceptionCheck");
          XS.Label(".AfterCCTorExceptionCheck");
        }
      }
      string xDataName = field;
      XS.Push(xDataName);
    }
  }
}
