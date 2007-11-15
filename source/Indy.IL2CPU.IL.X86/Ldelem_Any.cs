using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldelem_Any, true)]
	public class Ldelem_Any: Op {
		private int mElementSize;
		public Ldelem_Any(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xType = aInstruction.Operand as TypeReference;
			if (xType == null)
				throw new Exception("Unable to determine Type!");
			mElementSize = Engine.GetFieldStorageSize(xType);
		}

		// todo: refactor all Ldelem variants to use this method for emitting
		public static void Assemble(CPU.Assembler aAssembler, int aElementSize) {
			aAssembler.Add(new CPUx86.Pop("eax"));
			aAssembler.Add(new CPUx86.Move("edx", "0" + aElementSize.ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Multiply("edx"));
			aAssembler.Add(new CPUx86.Add("eax", "0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h"));
			aAssembler.Add(new CPUx86.Pop("edx"));
			aAssembler.Add(new CPUx86.Add("edx", "eax"));
			aAssembler.Add(new CPUx86.Move("eax", "edx"));
			int xSizeLeft = aElementSize;
			while(xSizeLeft > 0) {
				if(xSizeLeft >= 4) {
					aAssembler.Add(new CPUx86.Push("dword [eax]"));
					aAssembler.Add(new CPUx86.Add("eax", "4"));
					xSizeLeft -= 4;
				}else {
					if(xSizeLeft >= 2) {
						aAssembler.Add(new CPUx86.Move("ecx", "0"));
						aAssembler.Add(new CPUx86.Move("word ecx", "[eax]"));
						aAssembler.Add(new CPUx86.Push("ecx"));
						aAssembler.Add(new CPUx86.Add("eax", "2"));
						xSizeLeft -= 2;
					}else {
						if(xSizeLeft >= 1) {
							aAssembler.Add(new CPUx86.Move("ecx", "0"));
							aAssembler.Add(new CPUx86.Move("byte cl", "[eax]"));
							aAssembler.Add(new CPUx86.Push("ecx"));
							aAssembler.Add(new CPUx86.Add("eax", "1"));
							xSizeLeft -= 1;
						}else {
							throw new Exception("Size left: " + xSizeLeft);
						}
					}
				}
			}
			aAssembler.StackSizes.Pop();
			aAssembler.StackSizes.Pop();
			aAssembler.StackSizes.Push(aElementSize);
		}

		public override void DoAssemble() {
			Assemble(Assembler, mElementSize);
		}
	}
}