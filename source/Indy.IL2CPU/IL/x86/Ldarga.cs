using System;
using System.Linq;


using CPU = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldarga)]
	public class Ldarga: Op {
		private int mAddress;
		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddress = aMethodInfo.Arguments[aIndex].VirtualAddresses.First();
		}
		public Ldarga(MethodInformation aMethodInfo, int aIndex)
			: base(null, aMethodInfo) {
			SetArgIndex(aIndex, aMethodInfo);
		}

		public Ldarga(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			if (aReader != null) {
				SetArgIndex(aReader.OperandValueInt32, aMethodInfo);
				//ParameterDefinition xParam = aReader.Operand as ParameterDefinition;
				//if (xParam != null) {
				//    SetArgIndex(xParam.Sequence - 1, aMethodInfo);
				//}
			}
		}
		public override void DoAssemble() {
            new CPU.Push { DestinationReg = CPU.Registers.EBP };
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
            new CPU.Push { DestinationValue = (uint)mAddress };
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
			Add(Assembler);
		}
	}
}