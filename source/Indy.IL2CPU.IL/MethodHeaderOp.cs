using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	public abstract class MethodHeaderOp: Op {
		public MethodHeaderOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
	}
}