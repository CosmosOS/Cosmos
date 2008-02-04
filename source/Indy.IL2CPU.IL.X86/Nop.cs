using System;
using System.IO;
using Indy.IL2CPU.IL;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
    [OpCode(OpCodeEnum.Nop)]
    public class Nop : Op {
		public Nop(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
            // Assembler would be base type in IL
            // Cast to ours
            // var x = (X86.Assembler)Assembler
            // This would solve the threading issue
            // and later allow for operator overloads etc.
            // x.Noop();
			new CPU.Noop();
		}
	}
}