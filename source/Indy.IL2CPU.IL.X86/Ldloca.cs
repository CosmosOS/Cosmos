using System;
using System.Linq;

using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldloca)]
	public class Ldloca: Op {
		private string mAddress;
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

		public string Address {
			get {
				return mAddress;
			}
		}

		public sealed override void DoAssemble() {
			string[] xAddressParts = mAddress.Split('-');
			new CPUx86.Move(CPUx86.Registers.EDX, CPUx86.Registers.EBP);
			new CPUx86.Sub(CPUx86.Registers.EDX, xAddressParts[1]);
			new CPUx86.Push(CPUx86.Registers.EDX);
			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		}
	}
}