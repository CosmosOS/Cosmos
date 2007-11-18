using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

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
			mCtorName = CPU.Label.GenerateLabelName(xCtor);
			mIsReference = xTypeDef.IsClass;
			Engine.QueueMethod(xCtor);
			DoQueueMethod(GCImplementationRefs.AllocNewObjectRef);
		}

		public override void DoAssemble() {
			new CPU.Comment("Element Size = " + mElementSize);
			// element count is on the stack
			int xElementCountSize = Assembler.StackSizes.Peek();
			new CPUx86.Pop("edi");
			new CPUx86.Pushd("edi");
			Assembler.StackSizes.Push(xElementCountSize);
			new CPUx86.Pushd("0x" + mElementSize.ToString("X"));
			Assembler.StackSizes.Push(4);
			Multiply(Assembler);
			// the total items size is now on the stack
			new CPUx86.Pushd("0x" + (ObjectImpl.FieldDataOffset + 4).ToString("X"));
			Assembler.StackSizes.Push(4);
			Add(Assembler);
			// the total array size is now on the stack.
			Engine.QueueMethodRef(GCImplementationRefs.AllocNewObjectRef);
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef));
			Assembler.StackSizes.Pop();
			new CPUx86.Pushd("eax");
			new CPUx86.Pushd("eax");
			new CPUx86.Pushd("eax");
			new CPUx86.Pushd("eax");
			new CPUx86.Pushd("eax");
			Engine.QueueMethodRef(GCImplementationRefs.IncRefCountRef);
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));			
			Assembler.StackSizes.Push(4);
			new CPUx86.Pop("eax");
			Assembler.StackSizes.Push(4);
			new CPUx86.Move("dword [eax]", "0x" + Engine.RegisterType(Engine.GetTypeDefinition("mscorlib", "System.Array")).ToString("X"));
			new CPUx86.Add("eax", "4");
			new CPUx86.Move("dword [eax]", "0x" + InstanceTypeEnum.Array.ToString("X"));
			new CPUx86.Add("eax", "4");
			new CPUx86.Move("dword [eax]", "edi");
			new CPUx86.Call(mCtorName);
		}
	}
}