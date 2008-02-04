using System;
using System.Linq;

using CPU = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldc_R4)]
	public class Ldc_R4: Op {
		private Single mValue;
		public Ldc_R4(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mValue = aReader.OperandValueSingle;
		}
		public override void DoAssemble() {
			new CPU.Pushd("0x" + BitConverter.GetBytes(mValue).Aggregate("", (x, b) => x + b.ToString("X2")));
			Assembler.StackContents.Push(new StackContent(4, typeof(Single)));
		}
	}
}