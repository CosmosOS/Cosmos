using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using System.Collections.Generic;
using System.Reflection;
using Cosmos.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stloc)]
	public class Stloc : ILOp
	{
		public Stloc(Cosmos.Assembler.Assembler aAsmblr)
			: base(aAsmblr)
		{
		}

		public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
		{
			var xField = aOpCode as ILOpCodes.OpVar;
			var xFieldInfo = aMethod.MethodBase.GetMethodBody().LocalVariables[xField.Value];
			var xEBPOffset = ((int)GetEBPOffsetForLocal(aMethod, xField.Value));

            new Comment("EBPOffset = " + xEBPOffset);
			for (int i = (int)GetStackCountForLocal(aMethod, xFieldInfo) - 1; i >= 0; i--)
			{
				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
				new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement =(int)((0 - (xEBPOffset + (i * 4)))), SourceReg = CPUx86.Registers.EAX };
			}
		}

		// 		private bool mNeedsGC = false;
		// 		private MethodInformation.Variable mLocal;
		// 		private string mBaseLabel;
		// 
		// 		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
		// 			mLocal = aMethodInfo.Locals[aIndex];
		// 			mNeedsGC = aMethodInfo.Locals[aIndex].IsReferenceType;
		// 			mNeedsGC &= aMethodInfo.Locals[aIndex].VariableType.FullName != "System.String";
		// 		}
		// 
		// 		public Stloc(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			int xLocalIndex;
		// 			mBaseLabel = GetInstructionLabel(aReader);
		// 			xLocalIndex = aReader.OperandValueInt32;
		// 			SetLocalIndex(xLocalIndex, aMethodInfo);
		// 			//VariableDefinition xVarDef = aReader.Operand as VariableDefinition;
		// 			//if (xVarDef != null) {
		// 			//    SetLocalIndex(xVarDef.Index, aMethodInfo);
		// 			//}
		// 		}
		// 
		// 		public sealed override void DoAssemble() {
		// 			if (mNeedsGC) {
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = mLocal.VirtualAddresses[0] };
		//                 new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
		// 			}
		// 			foreach (int i in mLocal.VirtualAddresses.Reverse()) {
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; ;
		//                 new CPUx86.Move { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = i, SourceReg = CPUx86.Registers.EAX };
		// 			}
		// 			// no need to inc again, items on the transient stack are also counted
		// 			Assembler.Stack.Pop();
		// 		}
		// 	}
		// }

	}
}
