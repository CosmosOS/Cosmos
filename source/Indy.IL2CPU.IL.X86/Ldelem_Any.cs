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

		public static void Assemble(CPU.Assembler aAssembler, int aElementSize) {
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Move(CPUx86.Registers.EDX, "0" + aElementSize.ToString("X") + "h");
			new CPUx86.Multiply(CPUx86.Registers.EDX);
			new CPUx86.Add(CPUx86.Registers.EAX, "0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h");
			new CPUx86.Pop(CPUx86.Registers.EDX);
			new CPUx86.Add(CPUx86.Registers.EDX, CPUx86.Registers.EAX);
			new CPUx86.Move(CPUx86.Registers.EAX, CPUx86.Registers.EDX);
			int xSizeLeft = aElementSize;
			while(xSizeLeft > 0) {
				if(xSizeLeft >= 4) {
					new CPUx86.Push("dword", CPUx86.Registers.AtEAX);
					new CPUx86.Add(CPUx86.Registers.EAX, "4");
					xSizeLeft -= 4;
				}else {
					if(xSizeLeft >= 2) {
						new CPUx86.Move(CPUx86.Registers.ECX, "0");
						new CPUx86.Move("word", CPUx86.Registers.CX, CPUx86.Registers.AtEAX);
						new CPUx86.Push(CPUx86.Registers.ECX);
						new CPUx86.Add(CPUx86.Registers.EAX, "2");
						xSizeLeft -= 2;
					}else {
						if(xSizeLeft >= 1) {
							new CPUx86.Move(CPUx86.Registers.ECX, "0");
							new CPUx86.Move("byte", CPUx86.Registers.CL, CPUx86.Registers.AtEAX);
							new CPUx86.Push(CPUx86.Registers.ECX);
							new CPUx86.Add(CPUx86.Registers.EAX, "1");
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