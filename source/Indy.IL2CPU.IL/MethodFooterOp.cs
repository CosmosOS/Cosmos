using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class MethodFooterOp: Op {
		public const string EndOfMethodLabelNameNormal = ".END__OF__METHOD_NORMAL";
		public const string EndOfMethodLabelNameException = ".END__OF__METHOD_EXCEPTION";

		public MethodFooterOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
	}
}
