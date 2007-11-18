using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldobj)]
	public class Ldobj: Op {
		public Ldobj(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xTypeRef = aInstruction.Operand as TypeReference;
			if (xTypeRef == null) {
				throw new Exception("Type specification not found!");
			}
			TypeDefinition xTypeDef = Engine.GetDefinitionFromTypeReference(xTypeRef);
			mSize = Engine.GetFieldStorageSize(xTypeDef);
		}

		private int mSize;
		public override void DoAssemble() {
			new CPUx86.Pop(CPUx86.Registers.EAX);
			for (int i = 0; i < (mSize / 4); i++) {
				new CPUx86.Pushd(CPUx86.Registers.AtEAX);
				new CPUx86.Add(CPUx86.Registers.EAX, "4");
			}
			switch (mSize % 4) {
				case 1: {
						new CPUx86.Push("byte", CPUx86.Registers.AtEAX);
						break;
					}
				case 2: {
						new CPUx86.Push("word", CPUx86.Registers.AtEAX);
						break;
					}
				case 0: {
						break;
					}
				default: {
						throw new Exception("Remainder not supported!");
					}
			}
			Assembler.StackSizes.Pop();
			Assembler.StackSizes.Push(mSize);
		}
	}
}