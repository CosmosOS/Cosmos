using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldloca)]
	public class Ldloca: ILOp
	{
		public Ldloca(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpVar = (OpVar)aOpCode;
      uint xAddress = 0;
      var xBody = aMethod.MethodBase.GetMethodBody();
      for(int i = 0; i < xOpVar.Value;i++){
        var xLocal = xBody.LocalVariables[i];

        var xSize = Align(SizeOfType(xLocal.LocalType), 4);
        xAddress += xSize;
      }
      // xAddress contains full size of locals, excluding the actual local
      xAddress = 4 + xAddress;
      new CPUx86.Move {
        DestinationReg = CPUx86.Registers.EAX,
        SourceReg = CPUx86.Registers.EBP
      };
      new CPUx86.Sub {
        DestinationReg = CPUx86.Registers.EAX,
        SourceValue = xAddress
      };
      new CPUx86.Push {
        DestinationReg = CPUx86.Registers.EBX,
      };
      Assembler.Stack.Push(4);
    }

    
		// using System;
		// using System.Linq;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldloca)]
		// 	public class Ldloca: Op {
		// 		private int mAddress;
		// 		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
		// 			mAddress = aMethodInfo.Locals[aIndex].VirtualAddresses.LastOrDefault();
		// 		}
		// 		public Ldloca(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			SetLocalIndex(aReader.OperandValueInt32, aMethodInfo);
		// 			//    return;
		// 			//}
		// 			//VariableDefinition xVarDef = aReader.Operand as VariableDefinition;
		// 			//if (xVarDef != null) {
		// 			//    mIsReferenceTypeField = xVarDef.VariableType.IsClass;
		// 			//    SetLocalIndex(xVarDef.Index, aMethodInfo);
		// 			//}
		// 		}
		// 
		// 		public int Address {
		// 			get {
		// 				return mAddress;
		// 			}
		// 		}
		// 
		// 		public sealed override void DoAssemble() {
		// 			new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP };
		//             new CPUx86.Sub { DestinationReg = CPUx86.Registers.EDX, SourceValue = (uint)(Address * -1) };
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
		// 			Assembler.Stack.Push(new StackContent(4, true, false, false));
		// 		}
		// 	}
		// }
		
	}
}
