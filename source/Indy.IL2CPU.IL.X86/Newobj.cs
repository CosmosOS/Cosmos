using System;
using System.Linq;
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
		}

		public static void Assemble(Assembler.Assembler aAssembler, int aObjectSize, MethodDefinition aCtorDef, int aTypeId) {
			if (aCtorDef != null) {
				Engine.QueueMethodRef(aCtorDef);
			}
			Engine.QueueMethodRef(GCImplementationRefs.AllocNewObjectRef);
			Engine.QueueMethodRef(GCImplementationRefs.IncRefCountRef);
			int xExtraSize = 0;
			if (!aAssembler.InMetalMode) {
				xExtraSize = 4;
			}
			new CPUx86.Pushd("0" + (aObjectSize + xExtraSize).ToString("X").ToUpper() + "h");
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef));
			new CPUx86.Pushd(CPUx86.Registers.EAX);
			new CPUx86.Pushd(CPUx86.Registers.EAX);
			new CPUx86.Pushd(CPUx86.Registers.EAX);
			new CPUx86.Pushd(CPUx86.Registers.EAX);
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			int xObjSize = 0;
			int xGCFieldCount = (from item in Engine.GetTypeFieldInfo(aCtorDef, out xObjSize).Values
								 where item.NeedsGC
								 select item).Count();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new Move("dword", CPUx86.Registers.AtEAX, "0" + aTypeId.ToString("X") + "h");
			new Move("dword", "[eax + 4]", "0" + InstanceTypeEnum.NormalObject.ToString("X") + "h");
			new Move("dword", "[eax + 8]", "0x" + xGCFieldCount.ToString("X"));
			if (aCtorDef != null) {
				for (int i = 0; i < aCtorDef.Parameters.Count; i++) {
					new CPUx86.Pushd("[esp + 0x8]");
				}
				new CPUx86.Call(CPU.Label.GenerateLabelName(aCtorDef));
				//new CPUx86.Pop(CPUx86.Registers.EAX);
//				aAssembler.StackSizes.Pop();
				for (int i = 0; i < aCtorDef.Parameters.Count; i++) {
					new CPUx86.Add(CPUx86.Registers.ESP, "4");
					aAssembler.StackSizes.Pop();
				}
			}
			new CPUx86.Push(CPUx86.Registers.EAX);
			aAssembler.StackSizes.Push(4);
		}
	}
}