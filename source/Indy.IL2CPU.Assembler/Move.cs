using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	public class Move: Instruction {
		public readonly string Destination;
		public readonly string Source;
		public Move(string aDestination, string aSource) {
			Destination = aDestination;
			Source = aSource;
		}

		public override string ToString() {
			return "mov " + Destination + "," + Source;
		}
	}
}
