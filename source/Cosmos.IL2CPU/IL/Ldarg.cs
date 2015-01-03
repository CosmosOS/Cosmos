using System;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.X86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldarg )]
    public class Ldarg : ILOp
    {
        public Ldarg( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
          var xOpVar = (OpVar)aOpCode;
          DoExecute(Assembler, aMethod, xOpVar.Value);
        }

        public static int GetArgumentDisplacement(MethodInfo aMethod, ushort aParam) {
          var xMethodBase = aMethod.MethodBase;
          if (aMethod.PluggedMethod != null) {
            xMethodBase = aMethod.PluggedMethod.MethodBase;
          }
          var xMethodInfo = xMethodBase as SysReflection.MethodInfo;
          uint xReturnSize = 0;
          if (xMethodInfo != null) {
            xReturnSize = Align(SizeOfType(xMethodInfo.ReturnType), 4);
          }
          uint xOffset = 8;
          var xCorrectedOpValValue = aParam;
          if (!aMethod.MethodBase.IsStatic && aParam > 0) {
            // if the method has a $this, the OpCode value includes the this at index 0, but GetParameters() doesnt include the this
            xCorrectedOpValValue -= 1;
          }
          var xParams = xMethodBase.GetParameters();
          if (aParam == 0 && !xMethodBase.IsStatic) {
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
              uint xArgSize = xCurArgSize;
              foreach (var xParam in xParams)
              {
                xArgSize += Align(SizeOfType(xParam.ParameterType), 4);
              }
            for (int i = xParams.Length - 1; i >= aParam; i--) {
              var xSize = Align(SizeOfType(xParams[i].ParameterType), 4);
              xOffset += xSize;
            }

            if (xReturnSize > xArgSize)
            {
				uint xExtraSize = xReturnSize - xCurArgSize;
				xOffset += xExtraSize;
            }

            return (int)(xOffset + xCurArgSize - 4);
        } else {
            try
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
                xReturnSize = 0;

                if (xReturnSize > xArgSize)
                {
                    uint xExtraSize = xReturnSize - xArgSize;
                    xOffset += xExtraSize;
                }
                return (int)(xOffset + xCurArgSize - 4);
            }
            catch
            {
                throw;
            }
          }
        }

        public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aMethod, ushort aParam) {
          var xDisplacement = GetArgumentDisplacement(aMethod, aParam);
          Type xArgType;
          if (aMethod.MethodBase.IsStatic) {
            xArgType = aMethod.MethodBase.GetParameters()[aParam].ParameterType;
          } else {
            if (aParam == 0u) {
              xArgType = aMethod.MethodBase.DeclaringType;
              if (xArgType.IsValueType)
              {
                  xArgType = xArgType.MakeByRefType();
              }
            } else {
              xArgType = aMethod.MethodBase.GetParameters()[aParam - 1].ParameterType;
            }
          }
          new Comment("Ldarg");
          new Comment("Arg idx = " + aParam);
		  uint xArgRealSize = SizeOfType(xArgType);
          uint xArgSize = Align(xArgRealSize, 4);
		  new Comment("Arg type = " + xArgType.ToString());
		  new Comment("Arg real size = " + xArgRealSize + " aligned size = " + xArgSize);
		  if (xArgRealSize < 4)
		  {
				new CPUx86.MoveSignExtend { DestinationReg = CPUx86.Registers.EAX, Size = (byte)(xArgRealSize * 8), SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = xDisplacement };
				new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		  }
		  else
		  {
			  for (int i = 0; i < (xArgSize / 4); i++) {
				  new Push {
					  DestinationReg = Registers.EBP,
					  DestinationIsIndirect = true,
					  DestinationDisplacement = xDisplacement - (i * 4)
				  };
			  }
		  }
        }
    }
}
