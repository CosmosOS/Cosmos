using System;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

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
			int xStackSize = aElementSize;
			if(xStackSize % 4 != 0) {
				xStackSize += 4 - xStackSize % 4;
			}
			if(!aAssembler.InMetalMode) {
				new CPUx86.Pushd("[esp + " + (xStackSize + 4) + "]");
				Engine.QueueMethodRef(GCImplementationRefs.DecRefCountRef);
				new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
			}
			new CPUx86.Move("ebx", "[esp + " + xStackSize + "]"); // the index
			new CPUx86.Move("ecx", "[esp + " + (xStackSize + 4) + "]"); // the array
			new CPUx86.Add("ecx", "12");
			new CPUx86.Push("0x" + aElementSize.ToString("X"));
			aAssembler.StackSizes.Push(4);
			new CPUx86.Push("ebx");
			aAssembler.StackSizes.Push(4);
			Multiply(aAssembler);
			new CPUx86.Push("ecx");
			aAssembler.StackSizes.Push(4);
			Add(aAssembler);
			new CPUx86.Pop("ecx");
			aAssembler.StackSizes.Pop();
			for (int i = (aElementSize / 4) - 1; i >= 0; i -= 1) {
				new CPU.Comment("Start 1 dword");
				new CPUx86.Pop("ebx");
				new CPUx86.Move("[ecx]", "ebx");
				new CPUx86.Add("ecx", "4");
			}
			switch (aElementSize % 4) {
				case 1: {
						new CPU.Comment("Start 1 byte");
						new CPUx86.Pop("ebx");
						new CPUx86.Move("byte [ecx]", "bl");
						break;
					}
				case 2: {
						new CPU.Comment("Start 1 word");
						new CPUx86.Pop("ebx");
						new CPUx86.Move("word [ecx]", "bx");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (aElementSize % 4) + " not supported!");

			}
			new CPUx86.Add("esp", "0x8");
			aAssembler.StackSizes.Pop();
			aAssembler.StackSizes.Pop();
			aAssembler.StackSizes.Pop();
		}

		public override void DoAssemble() {
			Assemble(Assembler, mElementSize);
		}
	}
}