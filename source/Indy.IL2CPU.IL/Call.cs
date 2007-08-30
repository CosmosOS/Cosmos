using System;
using System.IO;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Call)]
	public class Call: Op {
		public override void Assemble(Instruction aInstruction, BinaryWriter aOutput) {
//			Console.WriteLine("Call, Operand = '{0}' ({1})", aInstruction.Operand, aInstruction.Operand == null ? "**NULL**" : aInstruction.Operand.GetType().FullName);
		}
	}
}