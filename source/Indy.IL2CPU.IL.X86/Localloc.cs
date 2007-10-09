using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Localloc)]
	public class Localloc: Op {
		public Localloc(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			//Call(new CPU.Label(RuntimeEngineRefs.Heap_AllocNewObjectRef).Name);
			//Pushd("eax");
			throw new NotImplementedException();

		}
	}
}