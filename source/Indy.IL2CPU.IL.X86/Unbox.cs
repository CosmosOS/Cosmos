using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Unbox)]
	public class Unbox: Op {
		private int mTypeId;
		private string mThisLabel;
		private string mNextOpLabel;
		private int mTypeSize;
		public Unbox(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xType = aInstruction.Operand as TypeReference;
			if(xType==null) {
				throw new Exception("Unable to determine Type!");
			}
			TypeDefinition xTypeDef = Engine.GetDefinitionFromTypeReference(xType);
			mTypeSize = Engine.GetFieldStorageSize(xTypeDef);
			mTypeId = Engine.RegisterType(xTypeDef);
			mThisLabel = GetInstructionLabel(aInstruction);
			mNextOpLabel = GetInstructionLabel(aInstruction.Next);
		}
		public override void DoAssemble() {
			string mReturnNullLabel = mThisLabel + "_ReturnNull";
			Pop("eax");
			Assembler.StackSizes.Pop();
			Compare("eax", "0");
			JumpIfZero(mReturnNullLabel);
			Pushd("[eax]", "0" + mTypeId + "h");
			Assembler.StackSizes.Push(4);
			Assembler.StackSizes.Push(4);
			MethodDefinition xMethodIsInstance = Engine.GetMethodDefinition(Engine.GetTypeDefinition("", "Indy.IL2CPU.VTablesImpl"), "IsInstance", "System.Int32", "System.Int32");
			Engine.QueueMethod(xMethodIsInstance);
			Op xOp = new Call(xMethodIsInstance);
			xOp.Assembler = Assembler;
			xOp.Assemble();
			Pop("eax");
			Assembler.StackSizes.Pop();
			Compare("eax", "0");
			JumpIfEquals(mReturnNullLabel);
			Pushd("eax");
			Assembler.StackSizes.Push(mTypeSize);
			JumpAlways(mNextOpLabel);
			Label(mReturnNullLabel);
			Pushd("0");
			Assembler.StackSizes.Push(4);
		}
	}
}