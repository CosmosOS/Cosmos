using System;
using System.Linq;

using CPU = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldc_R8)]
	public class Ldc_R8: Op {
		private readonly Double mValue;
		public Ldc_R8(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mValue = aReader.OperandValueDouble;
		}
		public override void DoAssemble() {
			byte[] xBytes = BitConverter.GetBytes(mValue);
			new CPU.Pushd("0x" + xBytes.Take(8).Aggregate("", (s, b) => s + b.ToString("X2")));
			new CPU.Pushd("0x" + xBytes.Skip(8).Aggregate("", (s, b) => s + b.ToString("X2")));
			Assembler.StackContents.Push(new StackContent(8, typeof(Double)));
		}
	}
}