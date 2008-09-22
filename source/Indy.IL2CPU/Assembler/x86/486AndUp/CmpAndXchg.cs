using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "cmpxchg")]
	public class CmpAndXchg: Instruction {
		public readonly string Destination;
		public readonly string Source;
		public CmpAndXchg(string aDestination, string aSource) {
			Destination = aDestination;
			Source = aSource;
		}

		public override string ToString() {
			return "cmpxchg " + Destination + ", " + Source;
		}
	}
}
