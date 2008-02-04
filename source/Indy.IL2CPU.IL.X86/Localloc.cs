using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Localloc)]
	public class Localloc: Op {
		public Localloc(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			//Call(new CPU.Label(RuntimeEngineRefs.Heap_AllocNewObjectRef).Name);
			//Pushd("eax");
			throw new NotImplementedException();

		}
	}
}