using System;
using System.IO;
using CPU = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldc_I8)]
	public class Ldc_I8: Op {
		private readonly long mValue;
		public Ldc_I8(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mValue = Int64.Parse(aReader.Operand.ToString());
		}
		public override void DoAssemble() {
			string theValue = mValue.ToString("X");
			new CPU.Pushd("0" + theValue.Substring(0, 8) + "h");
			new CPU.Pushd("0" + theValue.Substring(8) + "h");
			Assembler.StackContents.Push(new StackContent(8, typeof(long)));
		}
	}
}