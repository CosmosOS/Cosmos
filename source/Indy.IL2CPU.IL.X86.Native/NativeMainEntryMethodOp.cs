using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86.Native {
	public class NativeMainEntryMethodOp: X86MainEntryPointOp {
		public NativeMainEntryMethodOp(Instruction aInstruction, MethodInformation aMethodInfo): base(aInstruction, aMethodInfo) {
		}

		public override void Enter(string aName) {
			base.Enter(aName);
			Pushd("[ebp + 8]");
			Call(NativeOpCodeMap.Instance.GetGlueMethod(Cosmos.Kernel.Boot.Glue.GlueMethodTypeEnum.SaveBootInfoStruct));
		}

		public override void Exit() {
			NativeMethodFooterOp.AssembleFooter(0, mAssembler, new MethodInformation.Variable[0], new MethodInformation.Argument[0], 0);
		}
	}
}