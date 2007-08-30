using System;
using System.IO;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86 {
	internal class LdStr: IL.LdStr {
		public override void Assemble(Instruction aInstruction, BinaryWriter aOutput) {
			// Operand contains the string to be loaded
			//new CPU.
			Console.WriteLine("LdStr, string = '{0}'", aInstruction.Operand);
		}
	}
}