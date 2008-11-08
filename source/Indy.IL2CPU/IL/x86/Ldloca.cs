using System;
using System.Linq;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldloca)]
	public class Ldloca: Op {
		private int mAddress;
		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddress = aMethodInfo.Locals[aIndex].VirtualAddresses.LastOrDefault();
		}
		public Ldloca(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			SetLocalIndex(aReader.OperandValueInt32, aMethodInfo);
			//    return;
			//}
			//VariableDefinition xVarDef = aReader.Operand as VariableDefinition;
			//if (xVarDef != null) {
			//    mIsReferenceTypeField = xVarDef.VariableType.IsClass;
			//    SetLocalIndex(xVarDef.Index, aMethodInfo);
			//}
		}

		public int Address {
			get {
				return mAddress;
			}
		}

		public sealed override void DoAssemble() {
			new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP };
            new CPUx86.Sub { DestinationReg = CPUx86.Registers.EDX, SourceValue = (uint)(Address * -1) };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		}
	}
}