using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldelema, true)]
	public class Ldelema: Op {
		private int mElementSize;
		public Ldelema(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xTypeRef = aInstruction.Operand as TypeReference;
			TypeSpecification xTypeSpec = xTypeRef as TypeSpecification;
			if(xTypeSpec != null) {
				mElementSize = Engine.GetFieldStorageSize(xTypeRef);
			} else {
				mElementSize = 4;
			}
		}

		public static void Assemble(CPU.Assembler aAssembler, int aElementSize) {
			aAssembler.Add(new CPUx86.Pop("eax"));
			aAssembler.Add(new CPUx86.Move("edx", "0" + aElementSize.ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Multiply("edx"));
			aAssembler.Add(new CPUx86.Add("eax", "0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Pop("edx"));
			aAssembler.Add(new CPUx86.Add("edx", "eax"));
			aAssembler.Add(new CPUx86.Pushd("edx"));
		}

		public override void DoAssemble() {
			Assemble(Assembler, mElementSize);
		}
	}
}