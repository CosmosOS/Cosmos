using System;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldarg )]
    public class Ldarg : ILOp
    {
        public Ldarg( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
          var xOpVar = (OpVar)aOpCode;
          if (GetLabel(aMethod, aOpCode) == "System_Int32__Indy_IL2CPU_VTablesImpl_GetMethodAddressForType_System_Int32__System_Int32___DOT__000001AB") {
            Console.Write("");
          }
          DoExecute(Assembler, aMethod, xOpVar.Value);
        }

        public static int GetArgumentDisplacement(MethodInfo aMethod, ushort aParam) {
          var xMethodBase = aMethod.MethodBase;
          if (aMethod.PluggedMethod != null) {
            xMethodBase = aMethod.PluggedMethod.MethodBase;
          }
          var xMethodInfo = xMethodBase as System.Reflection.MethodInfo;
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
            var xCurArgSize = Align(SizeOfType(xMethodBase.DeclaringType), 4);
            for (int i = xParams.Length - 1; i >= aParam; i--) {
              var xSize = Align(SizeOfType(xParams[i].ParameterType), 4);
              xOffset += xSize;
            }
            uint xExtraSize = 0;
            if (xReturnSize > xCurArgSize) {
              xExtraSize = xCurArgSize - xReturnSize;
            }
            xOffset += xExtraSize;

            return (int)(xOffset + xCurArgSize - 4);
          } else {
            for (int i = xParams.Length - 1; i > xCorrectedOpValValue; i--) {
              var xSize = Align(SizeOfType(xParams[i].ParameterType), 4);
              xOffset += xSize;
            }
            var xCurArgSize = Align(SizeOfType(xParams[xCorrectedOpValValue].ParameterType), 4);
            uint xArgSize = 0;
            foreach (var xParam in xParams) {
              xArgSize += Align(SizeOfType(xParam.ParameterType), 4);
            }
            xReturnSize = 0;
            uint xExtraSize = 0;
            if (xReturnSize > xArgSize) {
              xExtraSize = xArgSize - xReturnSize;
            }
            xOffset += xExtraSize;

            return (int)(xOffset + xCurArgSize - 4);
          }
        }

        public static void DoExecute(Assembler Assembler, MethodInfo aMethod, ushort aParam) {
          uint xArgSize = 0;
          var xDisplacement = GetArgumentDisplacement(aMethod, aParam);
          Type xArgType;
          if (aMethod.MethodBase.IsStatic) {
            xArgType = aMethod.MethodBase.GetParameters()[aParam].ParameterType;
          } else {
            if (aParam == 0) {
              xArgType = aMethod.MethodBase.DeclaringType;
            } else {
              xArgType = aMethod.MethodBase.GetParameters()[aParam - 1].ParameterType;
            }
          }
          xArgSize = Align(SizeOfType(xArgType), 4);
          for (int i = 0; i < (xArgSize / 4); i++) {
            new Push {
              DestinationReg = Registers.EBP,
              DestinationIsIndirect = true,
              DestinationDisplacement = (int)(xDisplacement - ((i) * 4))
            };
          }
          Assembler.Stack.Push((int)xArgSize, xArgType);
        }


      // using System;
      // using System.Collections.Generic;
      // using System.IO;
      // 
      // using CPU = Cosmos.IL2CPU.X86;
      // 
      // namespace Indy.IL2CPU.IL.X86 {
      // 	[OpCode(OpCodeEnum.Ldarg)]
      // 	public class Ldarg: Op {
      // 		private MethodInformation.Argument mArgument;
      // 		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
      // 			mArgument = aMethodInfo.Arguments[aIndex];
      // 		}
      // 
      // 		public Ldarg(MethodInformation aMethodInfo, int aIndex)
      // 			: base(null, aMethodInfo) {
      // 			SetArgIndex(aIndex, aMethodInfo);
      // 		}
      // 
      // 		public Ldarg(ILReader aReader, MethodInformation aMethodInfo)
      // 			: base(aReader, aMethodInfo) {
      // 			int xArgIndex;
      // 			if (aReader != null) {
      // 				xArgIndex = aReader.OperandValueInt32;
      // 				SetArgIndex(xArgIndex, aMethodInfo);
      // 				//ParameterDefinition xParam = aReader.Operand as ParameterDefinition;
      // 				//if (xParam != null) {
      // 				//    SetArgIndex(xParam.Sequence - 1, aMethodInfo);
      // 				//}
      // 			}
      // 		}
      // 
      // 		public override void DoAssemble() {
      // 			Ldarg(Assembler, mArgument);
      // 		}
      // 	}
      // }

    }
}
