using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Asm = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Newobj, false)]
	public class Newobj: Op {
		public MethodReference CtorRef;
		public Newobj()
			: base(null, null) {
		}

		public Newobj(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			CtorRef = (MethodReference)aInstruction.Operand;
		}

		public override void DoAssemble() {
			Engine.QueueMethodRef(CtorRef);
			DoQueueMethod(RuntimeEngineRefs.Heap_AllocNewObjectRef);
			uint xObjectSize = ObjectUtilities.GetObjectStorageSize(Engine.GetDefinitionFromTypeReference(CtorRef.DeclaringType));
			Pushd("0" + xObjectSize.ToString("X").ToUpper() + "h");
			Call(new CPU.Label(RuntimeEngineRefs.Heap_AllocNewObjectRef).Name);
			Pushd("eax");
			//			Move(Assembler, "ecx", "eax");
			Pushd("eax");
			Move(Assembler, "dword [eax + 4]", "0" + InstanceTypeEnum.NormalObject.ToString("X") + "h");
			//Pushd("ecx");
			for (int i = 0; i < CtorRef.Parameters.Count; i++) {
				Pushd("[ebp - 010h]");
			}
			Call(new CPU.Label(CtorRef).Name);
			Pop("eax");
			for (int i = 0; i < CtorRef.Parameters.Count; i++) {
				Assembler.Add(new CPUx86.Add("esp", "4"));
			}
			Pushd("eax");
			//Assembler.Add(new CPUx86.Add("esp", objSize.ToString()));
		}
	}
}