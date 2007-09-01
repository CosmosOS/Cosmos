using System;
using System.IO;
using Indy.IL2CPU.IL;
using Cil = Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
    [OpCode(Cil.Code.Nop)]
    public class Nop : Op {
		public override void Assemble(Cil.Instruction aInstruction) {
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