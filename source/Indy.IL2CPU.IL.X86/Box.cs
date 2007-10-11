using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Box, false)]
	public class Box: Op {
		private int mTheSize;
		private int mTypeId;

		public Box(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xTypeRef = aInstruction.Operand as TypeReference;
			if (xTypeRef == null) {
				throw new Exception("Couldn't determine Type!");
			}
			if (xTypeRef is GenericParameter) {
				// todo: implement support for generics
				mTheSize = 4;
			} else {
				mTheSize = Engine.GetFieldStorageSize(xTypeRef);
			}
			if (((mTheSize / 4) * 4) != mTheSize) {
				throw new Exception("Incorrect Datasize. ( ((mTheSize / 4) * 4) === mTheSize should evaluate to true!");
			}
			//if (!(xTypeRef is GenericParameter)) {
			mTypeId = Engine.RegisterType(Engine.GetDefinitionFromTypeReference(xTypeRef));
			//}
		}

		public override void DoAssemble() {
			Assembler.Add(new CPUx86.Pushd("0" + (mTheSize + ObjectImpl.FieldDataOffset).ToString("X").ToUpper() + "h"));
			Call(new Label(RuntimeEngineRefs.Heap_AllocNewObjectRef).Name);
			Move(Assembler, "dword [eax]", "0" + mTypeId.ToString("X") + "h");
			Move(Assembler, "dword [eax + 4]", "0" + InstanceTypeEnum.BoxedValueType.ToString("X") + "h");
			for (int i = 0; i < (mTheSize / 4); i++) {
				Assembler.Add(new CPUx86.Pop("edx"));
				Move(Assembler, "dword [eax + 0" + (ObjectImpl.FieldDataOffset + (i * 4)).ToString("X") + "h]", "edx");
			}
			Assembler.StackSizes.Pop();
			Pushd(4, "eax");
			Assembler.StackSizes.Push(4);
		}
	}
}