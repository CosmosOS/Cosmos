using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldloc)]
	public class Ldloc : ILOp
	{
		public Ldloc(Cosmos.IL2CPU.Assembler aAsmblr)
			: base(aAsmblr)
		{
		}

		public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
		{
			var xOpVar = (OpVar)aOpCode;
			var xVar = aMethod.MethodBase.GetMethodBody().LocalVariables[xOpVar.Value];
			var xStackCount = GetStackCountForLocal(aMethod, xVar);
			var xEBPOffset = (int)GetEBPOffsetForLocal(aMethod, xOpVar);
			var xSize = SizeOfType(xVar.LocalType);
			if (xStackCount > 1)
			{
				for (int i = 0; i < xStackCount; i++)
				{
					new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = (int)(xEBPOffset + (i * 4)) };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
				}
			}
			else
			{
				new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };

				switch (xSize)
				{
					case 1:
						{
							new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = xEBPOffset };
							break;
						}
					case 2:
						{
							new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = xEBPOffset };

							break;
						}
					case 4:
						{
							new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = xEBPOffset };
							break;
						}
				}
				new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
				if(!xVar.LocalType.IsValueType)
				{
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
					new CPUx86.Call { DestinationLabel =MethodAndTypeLabelsHolder.GC_IncRefLabel };
				}
			}
			Assembler.Stack.Push((int)xSize, xVar.LocalType);
		}


		// using System;
		// using System.IO;
		// using Indy.IL2CPU.Assembler;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldloc)]
		// 	public class Ldloc: Op {
		// 		private MethodInformation.Variable mLocal;
		// 		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
		// 			mLocal = aMethodInfo.Locals[aIndex];
		// 		}
		// 		public Ldloc(MethodInformation aMethodInfo, int aIndex)
		// 			: base(null, aMethodInfo) {
		// 			SetLocalIndex(aIndex, aMethodInfo);
		// 		}
		// 
		// 		public Ldloc(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			SetLocalIndex(aReader.OperandValueInt32, aMethodInfo);
		// 			//VariableDefinition xVarDef = aReader.Operand as VariableDefinition;
		// 			//if (xVarDef != null) {
		// 			//    SetLocalIndex(xVarDef.Index, aMethodInfo);
		// 			//}
		// 		}
		// 
		// 		public sealed override void DoAssemble() {
		// 			Ldloc(Assembler, mLocal, GetService<IMetaDataInfoService>().SizeOfType(mLocal.VariableType));
		// 		}
		// 	}
		// }

	}
}
