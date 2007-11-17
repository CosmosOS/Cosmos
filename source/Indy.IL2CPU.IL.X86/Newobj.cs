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
			aAssembler.Add(new CPUx86.Pushd("0" + aObjectSize.ToString("X").ToUpper() + "h"));
			aAssembler.Add(new CPUx86.Call(new CPU.Label(RuntimeEngineRefs.Heap_AllocNewObjectRef).Name));
			aAssembler.Add(new CPUx86.Pushd("eax"));
			aAssembler.Add(new CPUx86.Pushd("eax"));
			aAssembler.StackSizes.Push(4);
			aAssembler.StackSizes.Push(4);
			Move(aAssembler, "dword [eax]", "0" + aTypeId.ToString("X") + "h");
			Move(aAssembler, "dword [eax + 4]", "0" + InstanceTypeEnum.NormalObject.ToString("X") + "h");
			if (aCtorDef != null) {
				for (int i = 0; i < aCtorDef.Parameters.Count; i++) {
					aAssembler.Add(new CPUx86.Pushd("[esp + 0x8]"));
				}
				aAssembler.Add(new CPUx86.Call(new CPU.Label(aCtorDef).Name));
				aAssembler.Add(new CPUx86.Pop("eax"));
				aAssembler.StackSizes.Pop();
				for (int i = 0; i < aCtorDef.Parameters.Count; i++) {
					aAssembler.Add(new CPUx86.Add("esp", "4"));
					aAssembler.StackSizes.Pop();
				}
			}
			aAssembler.Add(new CPUx86.Push("eax"));
			aAssembler.StackSizes.Push(4);
		}
	}
}