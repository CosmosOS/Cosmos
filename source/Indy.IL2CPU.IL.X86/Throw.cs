using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Throw, false)]
	public class Throw: Op {
		private MethodInformation mMethodInfo;
		public Throw(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			mMethodInfo = aMethodInfo;
		}

		public static void Assemble(Assembler.Assembler aAssembler, MethodInformation aMethodInfo) {
			new CPUx86.Pop("eax");
			new CPUx86.Move("[" + CPU.DataMember.GetStaticFieldName(CPU.Assembler.CurrentExceptionRef) + "]", "eax");
			new CPUx86.Move("ecx", "3");
			Call.EmitExceptionLogic(aAssembler, aMethodInfo, null, false);
			aAssembler.StackSizes.Pop();
		}
	
		public override void DoAssemble() {
			Assemble(Assembler, mMethodInfo);
		}
	}
}