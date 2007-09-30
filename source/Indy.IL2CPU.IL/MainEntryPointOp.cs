using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class MainEntryPointOp: Op {
		protected MainEntryPointOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		public abstract void Pushd(string aValue);
		public abstract void Call(MethodDefinition aMethod);

		public override void DoAssemble() {
			throw new NotImplementedException();
		}
	}
}
