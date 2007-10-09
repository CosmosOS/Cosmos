using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stelem_Any, true)]
	public class Stelem_Any: Op {
		private int mElementSize;
		public Stelem_Any(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xType = aInstruction.Operand as TypeReference;
			if (xType == null)
				throw new Exception("Unable to determine Type!");
			mElementSize = Engine.GetFieldStorageSize(xType);
			if (mElementSize % 4 != 0) {
				throw new Exception("ElementSize should be divisible by 4");
			}
		}

		// todo: refactor other Stelem variants to use this method
		public static void Assemble(CPU.Assembler aAssembler, int aElementSize) {
			// stack - 3 == the array
			// stack - 2 == the index
			// stack - 1 == the new value
			aAssembler.Add(new CPU.Literal("; Index at: [esp + " + aElementSize + "]"));
			aAssembler.Add(new CPU.Literal("; Array at: [esp + " + (aElementSize + 4) + "]"));
			aAssembler.Add(new CPUx86.Move("ebx", "[esp + " + aElementSize + "]")); // the index
			aAssembler.Add(new CPUx86.Move("ecx", "[esp + " + (aElementSize + 4) + "]")); // the array
			aAssembler.Add(new CPUx86.Add("ecx", "12"));
			Push(aAssembler, "0x"+aElementSize.ToString("X"));
			aAssembler.StackSizes.Push(4);
			Push(aAssembler, "ebx");
			aAssembler.StackSizes.Push(4);
			Multiply(aAssembler);
			Push(aAssembler, "ecx");
			aAssembler.StackSizes.Push(4);
			Add(aAssembler);
			aAssembler.Add(new CPUx86.Pop("ecx"));
			aAssembler.StackSizes.Pop();
			for (int i = (aElementSize / 4) - 1; i >= 0; i -= 1) {
				aAssembler.Add(new CPU.Literal("; start 1 dword"));
				aAssembler.Add(new CPUx86.Pop("ebx"));
				aAssembler.Add(new CPUx86.Move("[ecx]", "ebx"));
				aAssembler.Add(new CPUx86.Add("ecx", "4"));
			}
			aAssembler.Add(new CPUx86.Add("esp", "8"));
			//
			//			aAssembler.Add(new CPUx86.Move("[edx]", "ecx"));
			//
			//			for (int i = 0; i < (aElementSize % 4); i++) {
			//				aAssembler.Add(new CPUx86.Pushd("eax"));
			//				if (i != 0) {
			//					aAssembler.Add(new CPUx86.Add("eax", "4"));
			//					aAssembler.Add(new CPUx86.Pushd("eax"));
			//				}
			//			}
			aAssembler.StackSizes.Pop();
			aAssembler.StackSizes.Pop();
			aAssembler.StackSizes.Pop();
		}

		public override void DoAssemble() {
			Assemble(Assembler, mElementSize);
		}
	}
}