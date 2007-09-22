using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldelem_I4, false)]
	public class Ldelem_I4: Op {
		public Ldelem_I4(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("eax");
			Move(Assembler, "edx", "4");
			Assembler.Add(new CPU.Multiply("edx"));
			Pushd("eax");
			Pushd("0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h");
			Add();
			Pop("eax");
			Pop("edx");
			Assembler.Add(new CPU.Add("edx", "eax"));
			Move(Assembler, "eax", "[edx]");
			Pushd("eax");
		}
	}
}