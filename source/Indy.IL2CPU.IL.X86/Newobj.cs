using System;
using System.IO;
using Indy.IL2CPU.Assembler.X86;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Asm = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Newobj, false)]
	public class Newobj: Op {
		public MethodDefinition CtorDef;
		public Newobj()
			: base(null, null) {
		}

		public Newobj(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			CtorDef = Engine.GetDefinitionFromMethodReference((MethodReference)aInstruction.Operand);
		}

		public override void DoAssemble() {
			Assemble(Assembler, ObjectUtilities.GetObjectStorageSize(Engine.GetDefinitionFromTypeReference(CtorDef.DeclaringType)), CtorDef, Engine.RegisterTypeRef(CtorDef.DeclaringType));
			//Assembler.Add(new CPUx86.Add("esp", objSize.ToString()));
		}

		public static void Assemble(Assembler.Assembler aAssembler, int aObjectSize, MethodDefinition aCtorDef, int aTypeId) {
			if (aCtorDef != null) {
				Engine.QueueMethodRef(aCtorDef);
			}
			Engine.QueueMethodRef(RuntimeEngineRefs.Heap_AllocNewObjectRef);
			new CPUx86.Pushd("0" + aObjectSize.ToString("X").ToUpper() + "h");
			new CPUx86.Call(CPU.Label.GenerateLabelName(RuntimeEngineRefs.Heap_AllocNewObjectRef));
			new CPUx86.Pushd("eax");
			new CPUx86.Pushd("eax");
			aAssembler.StackSizes.Push(4);
			aAssembler.StackSizes.Push(4);
			new Move("dword [eax]", "0" + aTypeId.ToString("X") + "h");
			new Move("dword [eax + 4]", "0" + InstanceTypeEnum.NormalObject.ToString("X") + "h");
			if (aCtorDef != null) {
				for (int i = 0; i < aCtorDef.Parameters.Count; i++) {
					new CPUx86.Pushd("[esp + 0x8]");
				}
				new CPUx86.Call(CPU.Label.GenerateLabelName(aCtorDef));
				new CPUx86.Pop("eax");
				aAssembler.StackSizes.Pop();
				for (int i = 0; i < aCtorDef.Parameters.Count; i++) {
					new CPUx86.Add("esp", "4");
					aAssembler.StackSizes.Pop();
				}
			}
			new CPUx86.Push("eax");
			aAssembler.StackSizes.Push(4);
		}
	}
}