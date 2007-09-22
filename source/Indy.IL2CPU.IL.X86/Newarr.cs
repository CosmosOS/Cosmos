using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Newarr, false)]
	public class Newarr: Op {
		private uint mElementSize;
		private string mCtorName;

		public Newarr(TypeReference aTypeRef):base(null, null) {
			Initialize(aTypeRef);
		}

		public Newarr(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xTypeRef = aInstruction.Operand as TypeReference;
			if (xTypeRef == null) {
				throw new Exception("No TypeRef found!");
			}
			Initialize(xTypeRef);
		}

		private void Initialize(TypeReference aTypeRef) {
			mElementSize = Engine.GetFieldStorageSize(aTypeRef);
			TypeDefinition xArrayType = Engine.GetTypeDefinition("mscorlib", "System.Array");
			MethodDefinition xCtor = xArrayType.Constructors[0];
			mCtorName = new Assembler.Label(xCtor).Name;
			Engine.QueueMethod(xCtor);
			DoQueueMethod(RuntimeEngineRefs.Heap_AllocNewObjectRef);
		}

		public override void DoAssemble() {
			// element count is on the stack
			Pushd("0" + mElementSize.ToString("X") + "h");
			Multiply();
			// the total items size is now on the stack
			Pushd("0" + (ObjectImpl.FieldDataOffset+4).ToString("X") + "h");
			Add();
			// the total array size is now on the stack.
			Call(new Assembler.Label(RuntimeEngineRefs.Heap_AllocNewObjectRef).Name);
			Pushd("eax");
			Move(Assembler, "dword [eax + 4]", "0" + InstanceTypeEnum.Array.ToString("X") + "h");
			Pushd("eax");
			Call(mCtorName);
			Pop("eax");
			Pushd("eax");
		}
	}
}