using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldelem_Any, true)]
	public class Ldelem_Any: Op {
		private uint mElementSize;
		public Ldelem_Any(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xType = aInstruction.Operand as TypeReference;
			if (xType == null)
				throw new Exception("Unable to determine Type!");
			mElementSize = Engine.GetFieldStorageSize(xType);
		}

		// todo: refactor all Ldelem variants to use this method for emitting
		public static void Assemble(CPU.Assembler aAssembler, uint aElementSize) {
			aAssembler.Add(new CPUx86.Pop("eax"));
			aAssembler.Add(new CPUx86.Move("edx", "0" + aElementSize.ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Multiply("edx"));
			aAssembler.Add(new CPUx86.Add("eax", "0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Pop("edx"));
			aAssembler.Add(new CPUx86.Add("edx", "eax"));
			aAssembler.Add(new CPUx86.Move("eax", "[edx]"));
			aAssembler.Add(new CPUx86.Pushd("eax"));
		}

		public override void DoAssemble() {
			Assemble(Assembler, mElementSize);
		}
	}
}