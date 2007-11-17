using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

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
			new CPU.Pop("eax");
			for (int i = 0; i < (mSize / 4); i++) {
				new CPU.Pushd("[eax]");
				new CPU.Add("eax", "4");
			}
			switch (mSize % 4) {
				case 1: {
						new CPU.Push("byte [eax]");
						break;
					}
				case 2: {
						new CPU.Push("word [eax]");
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