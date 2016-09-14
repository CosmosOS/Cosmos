using System;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Starg)]
  public class Starg : ILOp
  {
    public Starg(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar)aOpCode;
      DoExecute(Assembler, aMethod, xOpVar.Value);
    }

    public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, ushort aParam)
    {
      var xDisplacement = Ldarg.GetArgumentDisplacement(aMethod, aParam);
      Type xArgType;
      if (aMethod.MethodBase.IsStatic)
      {
        xArgType = aMethod.MethodBase.GetParameters()[aParam].ParameterType;
      }
      else
      {
        if (aParam == 0u)
        {
          xArgType = aMethod.MethodBase.DeclaringType;
          if (xArgType.IsValueType)
          {
            xArgType = xArgType.MakeByRefType();
          }
        }
        else
        {
          xArgType = aMethod.MethodBase.GetParameters()[aParam - 1].ParameterType;
        }
      }

      XS.Comment("Arg idx = " + aParam);
      uint xArgRealSize = SizeOfType(xArgType);
      uint xArgSize = Align(xArgRealSize, 4);
      XS.Comment("Arg type = " + xArgType);
      XS.Comment("Arg real size = " + xArgRealSize + " aligned size = " + xArgSize);

      for (int i = (int)(xArgSize / 4) - 1; i >= 0; i--)
      {
        XS.Pop(EAX);
        XS.Set(EBP, EAX, destinationIsIndirect: true, destinationDisplacement: xDisplacement - (i * 4));
      }
    }
  }
}
