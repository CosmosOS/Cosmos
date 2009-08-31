using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Collections.Generic;
using System.Reflection;
using Indy.IL2CPU;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stloc)]
	public class Stloc : ILOp
	{
		public Stloc(Cosmos.IL2CPU.Assembler aAsmblr)
			: base(aAsmblr)
		{
		}

		private uint GetStackCount(MethodInfo aMethod, LocalVariableInfo aField)
		{
			var xSize = GetFieldStorageSize(aField.LocalType);
			var xResult = xSize / 4;
			if (xSize % 4 == 0)
			{
				xResult++;
			}
			return xResult;
		}

		private uint GetEBPOffsetForLocal(MethodInfo aMethod, OpVar aOp)
		{
			var xBody = aMethod.MethodBase.GetMethodBody();
			uint xOffset = 0;
			foreach (var xField in xBody.LocalVariables)
			{
				xOffset += GetStackCount(aMethod, xField);
			}
			return xOffset;
		}

		public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
		{
			var xField = aOpCode as ILOpCodes.OpVar;
			var xFieldInfo = aMethod.MethodBase.GetMethodBody().LocalVariables[xField.Value];
			var xEBPOffset = GetEBPOffsetForLocal(aMethod, xField);
			if (!xFieldInfo.LocalType.IsValueType)
			{
				new CPUx86.Push { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement =(int) xEBPOffset };
				new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
			}
			for (int i = (int)GetStackCount(aMethod, xFieldInfo) - 1; i >= 0; i--)
			{
				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; ;
				new CPUx86.Move { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement =(int)( xEBPOffset + (i * 4)), SourceReg = CPUx86.Registers.EAX };
			}
			// no need to inc again, items on the transient stack are also counted
			Assembler.Stack.Pop();
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
