using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Newarr, false)]
	public class Newarr: Op {
		private int mElementSize;
		private string mCtorName;
		private bool mIsReference;
		private string mBaseLabelName;

		public Newarr(TypeReference aTypeRef, string aBaseLabelName)
			: base(null, null) {
			Initialize(aTypeRef, aBaseLabelName);
		}

		public Newarr(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xTypeRef = aInstruction.Operand as TypeReference;
			if (xTypeRef == null) {
				throw new Exception("No TypeRef found!");
			}
			Initialize(xTypeRef, GetInstructionLabel(aInstruction));
		}

		private void Initialize(TypeReference aTypeRef, string aBaseLabelName) {
			mBaseLabelName = aBaseLabelName;
			TypeDefinition xTypeDef = Engine.GetDefinitionFromTypeReference(aTypeRef);
			mElementSize = Engine.GetFieldStorageSize(aTypeRef);
			TypeDefinition xArrayType = Engine.GetTypeDefinition("mscorlib", "System.Array");
			MethodDefinition xCtor = xArrayType.Constructors[0];
			mCtorName = new Assembler.Label(xCtor).Name;
			mIsReference = xTypeDef.IsClass;
			Engine.QueueMethod(xCtor);
			DoQueueMethod(RuntimeEngineRefs.Heap_AllocNewObjectRef);
		}

		public override void DoAssemble() {
			Literal("; Element Size = " + mElementSize);
			// element count is on the stack
			int xElementCountSize = Assembler.StackSizes.Peek();
			Pop("edi");
			Pushd(xElementCountSize, "edi");
			Pushd(4, "0" + mElementSize.ToString("X") + "h");
			Multiply();
			// the total items size is now on the stack
			Pushd(4, "0" + (ObjectImpl.FieldDataOffset + 4).ToString("X") + "h");
			Add();
			// the total array size is now on the stack.
			Call(new Assembler.Label(RuntimeEngineRefs.Heap_AllocNewObjectRef).Name);
			Assembler.StackSizes.Pop();
			Pushd(4, "eax");
			Pushd(4, "eax");
			Move(Assembler, "dword [eax]", "0" + Engine.RegisterType(Engine.GetTypeDefinition("mscorlib", "System.Array")).ToString("X") + "h");
			Assembler.Add(new CPU.Add("eax", "4"));
			Move(Assembler, "dword [eax]", "0" + InstanceTypeEnum.Array.ToString("X") + "h");
			Assembler.Add(new CPU.Add("eax", "4"));
			Move(Assembler, "dword [eax]", "edi");
			Call(mCtorName);
		}
	}
}