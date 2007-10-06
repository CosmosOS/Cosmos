using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stelem_Ref, true)]
	public class Stelem_Ref: Op {
		public Stelem_Ref(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public static void Assemble(CPU.Assembler aAssembler) {
			// stack - 3 == the array
			// stack - 2 == the index
			// stack - 1 == the new value
			aAssembler.Add(new CPUx86.Pop("ecx")); // the new value;
			aAssembler.Add(new CPUx86.Pop("eax")); // the index
			aAssembler.Add(new CPUx86.Move("edx", "4"));
			aAssembler.Add(new CPUx86.Multiply("edx"));
			aAssembler.Add(new CPUx86.Add("eax", "0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Pop("edx"));
			aAssembler.Add(new CPUx86.Add("edx", "eax"));
			aAssembler.Add(new CPUx86.Move("[edx]", "ecx"));
		}
		public override void DoAssemble() {
			Assemble(Assembler);
		}
	}
}