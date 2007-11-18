using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Initobj)]
	public class Initobj: Op {
		private int mObjSize;
		public Initobj(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xTypeRef = aInstruction.Operand as TypeReference;
			if (xTypeRef == null) {
				throw new Exception("Type not found!");
			}
			mObjSize = 0;
			if (xTypeRef.IsValueType) {
				Engine.GetTypeFieldInfo(Engine.GetDefinitionFromTypeReference(xTypeRef), out mObjSize);
			}
		}

		public override void DoAssemble() {
			Assembler.StackSizes.Pop();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			for (int i = 0; i < (mObjSize / 4); i++) {
				new CPUx86.Move("dword [eax + 0" + (i * 4).ToString("X") + "h]", "0");
			}
			switch (mObjSize % 4) {
				case 1: {
						new CPUx86.Move("byte [eax + 0" + ((mObjSize / 4) * 4).ToString("X") + "h]", "0");
						break;
					}
				case 2: {
						new CPUx86.Move("word [eax + 0" + ((mObjSize / 4) * 4).ToString("X") + "h]", "0");
						break;
					}
				case 0:
					break;
				default: {
						throw new Exception("Remainder size " + mObjSize % 4 + " not supported yet!");
					}
			}
		}
	}
}