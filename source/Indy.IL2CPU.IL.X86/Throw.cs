using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Throw, false)]
	public class Throw: Op {
		// TODO: When threading is being worked on, fix this to work multithreaded!
		public const string CurrentExceptionDataMember = "__CURRENT_EXCEPTION__";

		public Throw(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			if ((from item in Assembler.DataMembers
				 where item.Value.Name == CurrentExceptionDataMember
				 select item).Count() == 0) {
				Assembler.DataMembers.Add(new System.Collections.Generic.KeyValuePair<string, Indy.IL2CPU.Assembler.DataMember>("il2cpu-system", new Indy.IL2CPU.Assembler.DataMember(CurrentExceptionDataMember, "dd", "0")));
			}
			new CPU.Pop("eax");
			new CPU.Move("[" + CurrentExceptionDataMember + "]", "eax");
			new CPU.JumpAlways(MethodFooterOp.EndOfMethodLabelNameException);
			Assembler.StackSizes.Pop();
		}
	}
}