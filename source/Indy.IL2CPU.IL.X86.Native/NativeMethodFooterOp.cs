using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86.Native {
	public class NativeMethodFooterOp: X86MethodFooterOp {
		private string mLabelName;
		public NativeMethodFooterOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mLabelName = aMethodInfo.LabelName;
		}

		public override void DoAssemble() {
			base.DoAssemble();
		}
	}
}