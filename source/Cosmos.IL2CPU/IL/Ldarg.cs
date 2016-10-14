using System;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.X86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldarg)]
  public class Ldarg : ILOp
  {
    public Ldarg(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpVar = (OpVar) aOpCode;
      DoExecute(Assembler, aMethod, xOpVar.Value);
    }

    /// <summary>
    /// <para>This methods gives the full displacement for an argument. Arguments are in "reverse" order:
    /// <code>public static int Add(int a, int b)</code>
    /// In this situation, argument b is at EBP+8, argument A is at EBP+12
    /// </para>
    /// <para>
    /// After the method returns, the return value is on the stack. This means, that when the return size is larger than the
    /// total argument size, we need to reserve extra stack:
    /// <code>public static Int64 Convert(int value)</code>
    /// In this situation, argument <code>value</code> is at EBP+12
    /// </para>
    /// </summary>
    /// <param name="aMethod"></param>
    /// <param name="aParam"></param>
    /// <returns></returns>
    public static int GetArgumentDisplacement(MethodInfo aMethod, ushort aParam)
    {
      var xMethodBase = aMethod.MethodBase;
      if (aMethod.PluggedMethod != null)
      {
        xMethodBase = aMethod.PluggedMethod.MethodBase;
      }
      var xMethodInfo = xMethodBase as SysReflection.MethodInfo;
      uint xReturnSize = 0;
      if (xMethodInfo != null)
      {
        xReturnSize = Align(SizeOfType(xMethodInfo.ReturnType), 4);
      }
      uint xOffset = 8;
      var xCorrectedOpValValue = aParam;
      if (!aMethod.MethodBase.IsStatic && aParam > 0)
      {
        // if the method has a $this, the OpCode value includes the this at index 0, but GetParameters() doesnt include the this
        xCorrectedOpValValue -= 1;
      }
      var xParams = xMethodBase.GetParameters();
      if (aParam == 0 && !xMethodBase.IsStatic)
      {
        // return the this parameter, which is not in .GetParameters()
        uint xCurArgSize;
        if (xMethodBase.DeclaringType.IsValueType)
        {
          // value types get a reference passed to the actual value, so pointer:
          xCurArgSize = 4;
        }
        else
        {
          xCurArgSize = Align(SizeOfType(xMethodBase.DeclaringType), 4);
        }
        uint xArgSize = 0;
        foreach (var xParam in xParams)
        {
          xArgSize += Align(SizeOfType(xParam.ParameterType), 4);
        }
        if (!xMethodBase.IsStatic)
        {
          xArgSize += 4; // add $this pointer
        }
        for (int i = xParams.Length - 1; i >= aParam; i--)
        {
          var xSize = Align(SizeOfType(xParams[i].ParameterType), 4);
          xOffset += xSize;
        }
        if (xReturnSize > xArgSize)
        {
          uint xExtraSize = xReturnSize - xCurArgSize;
          xOffset += xExtraSize;
        }

        return (int) (xOffset + xCurArgSize - 4);
      }
      else
      {
        for (int i = xParams.Length - 1; i > xCorrectedOpValValue; i--)
        {
          var xSize = Align(SizeOfType(xParams[i].ParameterType), 4);
          xOffset += xSize;
        }
        var xCurArgSize = Align(SizeOfType(xParams[xCorrectedOpValValue].ParameterType), 4);
        uint xArgSize = 0;
        foreach (var xParam in xParams)
        {
          xArgSize += Align(SizeOfType(xParam.ParameterType), 4);
        }
        if (!xMethodBase.IsStatic)
        {
          xArgSize += 4; // add $this pointer
        }
        if (xReturnSize > xArgSize)
        {
          uint xExtraSize = xReturnSize - xArgSize;
          xOffset += xExtraSize;
        }
        return (int) (xOffset + xCurArgSize - 4);
      }
    }

    public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, ushort aParam)
    {
      var xDisplacement = GetArgumentDisplacement(aMethod, aParam);
      var xType = GetArgumentType(aMethod, aParam);
      uint xArgRealSize = SizeOfType(xType);
      uint xArgSize = Align(xArgRealSize, 4);

      XS.Comment("Arg idx = " + aParam);
      XS.Comment("Arg type = " + xType);
      XS.Comment("Arg real size = " + xArgRealSize + " aligned size = " + xArgSize);
      if (xArgRealSize < 4)
      {
        new MoveSignExtend
        {
          DestinationReg = RegistersEnum.EAX,
          Size = (byte) (xArgRealSize * 8),
          SourceReg = RegistersEnum.EBP,
          SourceIsIndirect = true,
          SourceDisplacement = xDisplacement
        };
        XS.Push(XSRegisters.EAX);
      }
      else
      {
        for (int i = 0; i < (xArgSize / 4); i++)
        {
          XS.Push(XSRegisters.EBP, isIndirect: true, displacement: (xDisplacement - (i * 4)));
        }
      }
    }

    public static Type GetArgumentType(MethodInfo aMethod, ushort aParam)
    {
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

      return xArgType;
    }
  }
}
