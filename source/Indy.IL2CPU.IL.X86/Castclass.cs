using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Castclass, false)]
	public class Castclass: Op {
		private int mTypeId;
		private string mThisLabel;
		private string mNextOpLabel;
		public Castclass(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TypeReference xType = aInstruction.Operand as TypeReference;
			if(xType==null) {
				throw new Exception("Unable to determine Type!");
			}
			TypeDefinition xTypeDef = Engine.GetDefinitionFromTypeReference(xType);
			mTypeId = Engine.RegisterType(xTypeDef);
			mThisLabel = GetInstructionLabel(aInstruction);
			mNextOpLabel = GetInstructionLabel(aInstruction.Next);
		}

		public override void DoAssemble() {
			// todo: throw an exception when the class does not support the cast!
			string mReturnNullLabel = mThisLabel + "_ReturnNull";
			Pop("eax");
			Compare("eax", "0");
			JumpIfZero(mReturnNullLabel);
			Pushd("[eax]", "0" + mTypeId + "h");
			MethodDefinition xMethodIsInstance = Engine.GetMethodDefinition(Engine.GetTypeDefinition("", "Indy.IL2CPU.VTablesImpl"), "IsInstance", "System.Int32", "System.Int32");
			Engine.QueueMethod(xMethodIsInstance);
			Call(new CPU.Label(xMethodIsInstance).Name);
			Compare("eax", "0");
			JumpIfEquals(mReturnNullLabel);
			Pushd("eax");
			JumpAlways(mNextOpLabel);
			Label(mReturnNullLabel);
			Pushd("0");
		}
	}
}