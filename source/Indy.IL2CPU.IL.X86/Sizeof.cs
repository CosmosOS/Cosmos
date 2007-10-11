using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Sizeof)]
	public class Sizeof: Op {
		private int mTheSize;
		public Sizeof(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xTypeRef = aInstruction.Operand as TypeReference;
			if (xTypeRef == null) {
				throw new Exception("Type not found!");}
			Engine.GetTypeFieldInfo(Engine.GetDefinitionFromTypeReference(xTypeRef), out mTheSize);
		}
		public override void DoAssemble() {
			Pushd(4, "0" + mTheSize.ToString("X") + "h");
		}
	}
}