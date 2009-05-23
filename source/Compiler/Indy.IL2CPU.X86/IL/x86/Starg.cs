using System;


using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Starg)]
	public class Starg: Op {
		private int[] mAddresses;
		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddresses = aMethodInfo.Arguments[aIndex].VirtualAddresses;

		}
		public Starg(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			SetArgIndex(aReader.OperandValueInt32, aMethodInfo);
		}
		public override void DoAssemble() {
			if (mAddresses == null || mAddresses.Length == 0) {
				throw new Exception("No Address Specified!");
			}
			for (int i = (mAddresses.Length - 1); i >= 0; i -= 1) {
				new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = mAddresses[i], SourceReg = CPUx86.Registers.EAX };
			}
			Assembler.StackContents.Pop();
		}
	}
}