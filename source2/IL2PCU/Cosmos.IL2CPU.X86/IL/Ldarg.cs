using System;
using Cosmos.IL2CPU.ILOpCodes;
using Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldarg)]
  public class Ldarg: ILOp {
    public Ldarg(Cosmos.IL2CPU.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpVar = (OpVar)aOpCode;
      var xMethodInfo = aMethod.MethodBase as System.Reflection.MethodInfo;
      uint xReturnSize = 0;
      if (xMethodInfo != null) {
        xReturnSize = Align(SizeOfType(xMethodInfo.ReturnType), 4);
      }
      uint xOffset = 12;
      var xCorrectedOpValValue = xOpVar.Value;
      if(!aMethod.MethodBase.IsStatic){
        // if the method has a $this, the OpCode value includes the this at index 0, but GetParameters() doesnt include the this
        xCorrectedOpValValue -= 1;
      }
      var xParams = aMethod.MethodBase.GetParameters();
      if (aOpCode.Position == 0 && !aMethod.MethodBase.IsStatic) {
        // return the this parameter, which is not in .GetParameters()
        var xCurArgSize = Align(SizeOfType(aMethod.MethodBase.DeclaringType), 4);
        for (int i = xParams.Length - 1; i > xOpVar.Value; i--) {
          var xSize = Align(SizeOfType(xParams[i].ParameterType), 4);
          xOffset += xSize;
        }
        uint xExtraSize = 0;
        if (xReturnSize > xCurArgSize) {
          xExtraSize = xCurArgSize - xReturnSize;
        }
        xOffset += xExtraSize;

        for (int i = 0; i < (xCurArgSize / 4); i++) {
          new Push {
            DestinationReg = Registers.EBP,
            DestinationIsIndirect = true,
            DestinationDisplacement = (int)(xCurArgSize - ((i + 1) * 4))
          };
        }
        Assembler.Stack.Push((int)xCurArgSize, aMethod.MethodBase.DeclaringType);
      } else {
        for (int i = xParams.Length - 1; i > xOpVar.Value; i--) {
          var xSize = Align(SizeOfType(xParams[i].ParameterType), 4);
          xOffset += xSize;
        }
        var xCurArgSize = Align(SizeOfType(xParams[xOpVar.Value].ParameterType), 4);
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

        for (int i = 0; i < (xCurArgSize / 4); i++) {
          new Push {
            DestinationReg = Registers.EBP,
            DestinationIsIndirect = true,
            DestinationDisplacement = (int)(xCurArgSize - ((i + 1) * 4))
          };
        }
        Assembler.Stack.Push((int)xCurArgSize, xParams[xOpVar.Value].ParameterType);
      }
    }


    // using System;
    // using System.Collections.Generic;
    // using System.IO;
    // 
    // using CPU = Indy.IL2CPU.Assembler.X86;
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
