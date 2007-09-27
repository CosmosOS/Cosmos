using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Initobj)]
	public class Initobj: Op {
		private uint mObjSize;
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
			if(((mObjSize/4)*4) != mObjSize) {
				throw new Exception("For now, value type size must be a multiplicative of 4!");
			}
		}

		public override void DoAssemble() {
			Pop("eax");
			for(int i = 0; i < (mObjSize/4);i++) {
				Move(Assembler, "[eax + 0" + i.ToString("X") + "h]", "0");
			}
		}
	}
}