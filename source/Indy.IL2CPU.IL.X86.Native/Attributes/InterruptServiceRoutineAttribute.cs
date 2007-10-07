using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class InterruptServiceRoutineAttribute: Attribute {
		public InterruptServiceRoutineAttribute(byte aNumber) {
			InterruptNumber = aNumber;
		}
		public readonly byte InterruptNumber;
	}
}