using System;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stsfld)]
	public class Stsfld: Op {
		private string mDataName;
		private int mSize;
		private TypeReference mDataType;
		private bool mNeedsGC;
		private string mBaseLabel;


		public Stsfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			mSize = Engine.GetFieldStorageSize(xField.FieldType);
			Engine.QueueStaticField(xField, out mDataName);
			mNeedsGC = !xField.FieldType.IsValueType && xField.FieldType.FullName != "System.String";
			mDataType = xField.FieldType;
			mBaseLabel = GetInstructionLabel(aInstruction);
		}

		public override void DoAssemble() {
			if (mNeedsGC) {
				new CPUx86.Pushd("[" + mDataName + "]");
				TypeSpecification xTypeSpec = mDataType as TypeSpecification;
				if (xTypeSpec != null) {
					TypeDefinition xElementDef = Engine.GetDefinitionFromTypeReference(xTypeSpec.ElementType);
					if ((!xElementDef.IsValueType) && xElementDef.FullName != "System.String") {
						new CPUx86.Compare("dword " + CPUx86.Registers.AtESP, "0");
						new CPUx86.JumpIfEquals(mBaseLabel + "_GC_LOCAL_CLEANUP_ENTRY_VAR_END");
						new CPUx86.Pushd("[" + mDataName + "]");
						new CPUx86.Push("8");
						Op.Add(Assembler);
						new CPUx86.Pop(CPUx86.Registers.EDX); // total item count	address
						new CPUx86.Move(CPUx86.Registers.EBX, CPUx86.Registers.AtEDX);
						new CPUx86.Add(CPUx86.Registers.EDX, "4");
						new CPUx86.Move(CPUx86.Registers.ECX, "0"); // counter
						new Label(mBaseLabel + "_GC_LOCAL_CLEANUP_ENTRY_VAR");
						new CPUx86.Compare(CPUx86.Registers.EBX, CPUx86.Registers.ECX);
						new CPUx86.JumpIfEquals(mBaseLabel + "_GC_LOCAL_CLEANUP_ENTRY_VAR_END");
						new CPUx86.Push(CPUx86.Registers.EDX);
						new CPUx86.Push(CPUx86.Registers.ECX);
						new CPUx86.Push(CPUx86.Registers.EBX);
						new CPUx86.Push("dword", CPUx86.Registers.AtEDX);
						new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
						new CPUx86.Pop(CPUx86.Registers.EBX);
						new CPUx86.Pop(CPUx86.Registers.ECX);
						new CPUx86.Pop(CPUx86.Registers.EDX);
						new CPUx86.Add(CPUx86.Registers.EDX, "4");
						new CPUx86.Add(CPUx86.Registers.ECX, "1");
						new CPUx86.JumpAlways(mBaseLabel + "_GC_LOCAL_CLEANUP_ENTRY_VAR");
						new Label(mBaseLabel + "_GC_LOCAL_CLEANUP_ENTRY_VAR_END");
					}
				}
				Engine.QueueMethodRef(GCImplementationRefs.DecRefCountRef);
				new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
			}
			for (int i = 1; i <= (mSize / 4); i++) {
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Move("dword [" + mDataName + " + 0x" + (mSize - (i * 4)).ToString("X") + "]", "eax");
			}
			switch (mSize % 4) {
				case 1: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Move("byte [" + mDataName + "]", "al");
						break;
					}
				case 2: {
						new CPUx86.Pop(CPUx86.Registers.EAX);
						new CPUx86.Move("word [" + mDataName + "]", "ax");
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (mSize % 4) + " not supported!");

			}
			Assembler.StackSizes.Pop();
		}
	}
}