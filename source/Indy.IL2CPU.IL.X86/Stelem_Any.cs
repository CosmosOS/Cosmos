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
			int xStackSize = aElementSize;
			if(xStackSize % 4 != 0) {
				xStackSize += 4 - xStackSize % 4;
			}
			aAssembler.Add(new CPU.Comment("; Index at: [esp + " + xStackSize + "]"));
			aAssembler.Add(new CPU.Comment("; Array at: [esp + " + (xStackSize + 4) + "]"));
			aAssembler.Add(new CPUx86.Move("ebx", "[esp + " + xStackSize + "]")); // the index
			aAssembler.Add(new CPUx86.Move("ecx", "[esp + " + (xStackSize + 4) + "]")); // the array
			aAssembler.Add(new CPUx86.Add("ecx", "12"));
			Push(aAssembler, 4, "0x" + aElementSize.ToString("X"));
			Push(aAssembler, 4, "ebx");
			Multiply(aAssembler);
			Push(aAssembler, 4, "ecx");
			Add(aAssembler);
			aAssembler.Add(new CPUx86.Pop("ecx"));
			aAssembler.StackSizes.Pop();
			for (int i = (aElementSize / 4) - 1; i >= 0; i -= 1) {
				aAssembler.Add(new CPU.Comment("Start 1 dword"));
				aAssembler.Add(new CPUx86.Pop("ebx"));
				aAssembler.Add(new CPUx86.Move("[ecx]", "ebx"));
				aAssembler.Add(new CPUx86.Add("ecx", "4"));
			}
			switch (aElementSize % 4) {
				case 1: {
						aAssembler.Add(new CPU.Comment("Start 1 byte"));
						aAssembler.Add(new CPUx86.Pop("ebx"));
						Move(aAssembler, "byte [ecx]", "bl");
						break;
					}
				case 2: {
						aAssembler.Add(new CPU.Comment("Start 1 word"));
						aAssembler.Add(new CPUx86.Pop("ebx"));
						Move(aAssembler, "word [ecx]", "bx");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (aElementSize % 4) + " not supported!");

			}
			aAssembler.Add(new CPUx86.Add("esp", "0x8"));
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