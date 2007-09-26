using System;
using System.IO;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldtoken)]
	public class Ldtoken: Op {
		private string mTokenAddress;
		public Ldtoken(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldDefinition xFieldDef = aInstruction.Operand as FieldDefinition;
			if (xFieldDef != null) {
				if (!xFieldDef.IsStatic) {
					throw new Exception("Nonstatic field-backed tokens not supported yet!");
				}
				Engine.QueueStaticField(xFieldDef);
				mTokenAddress = DataMember.GetStaticFieldName(xFieldDef);
				return;
			}
			TypeReference xTypeRef = aInstruction.Operand as TypeReference;
			if(xTypeRef!=null) {
				mTokenAddress = "0" + Engine.RegisterType(Engine.GetDefinitionFromTypeReference(xTypeRef)).ToString("X") + "h";
				return;
			}
			throw new Exception("Token type not supported yet!");
		}

		public override void DoAssemble() {
			Pushd(mTokenAddress);
		}
	}
}