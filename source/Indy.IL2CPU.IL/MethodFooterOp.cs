using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class MethodFooterOp: Op {
		public const string EndOfMethodLabelName = ".END__OF__METHOD";

		public MethodFooterOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
	}
}
