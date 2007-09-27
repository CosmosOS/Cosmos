using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stelem_Any, false)]
	public class Stelem_Any: Op {
		private uint mElementSize;
		public Stelem_Any(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xType = aInstruction.Operand as TypeReference;
			if (xType == null)
				throw new Exception("Unable to determine Type!");
			mElementSize = Engine.GetFieldStorageSize(xType);
		}

		// todo: refactor other Stelem variants to use this method
		public static void Assemble(CPU.Assembler aAssembler, uint aElementSize) {
			// stack - 3 == the array
			// stack - 2 == the index
			// stack - 1 == the new value
			aAssembler.Add(new CPUx86.Pop("ecx")); // the new value;
			aAssembler.Add(new CPUx86.Pop("eax")); // the index
			aAssembler.Add(new CPUx86.Move("edx", "0" + aElementSize.ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Multiply("edx"));
			aAssembler.Add(new CPUx86.Add("eax", "0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Pop("edx"));
			aAssembler.Add(new CPUx86.Add("edx", "eax"));
			aAssembler.Add(new CPUx86.Move("[edx]", "ecx"));
		}

		public override void DoAssemble() {
			Assemble(Assembler, mElementSize);
		}
	}
}