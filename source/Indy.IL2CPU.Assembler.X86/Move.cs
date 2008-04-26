using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "mov")]
	public class Move: Instruction {
		public readonly string Destination;
		public readonly string Source;
		public readonly string Size;
		
        public Move(string aSize, string aDestination, string aSource) : this(aDestination, aSource) {
			Size = aSize;
		}

        public Move(string aDestination, string aSource) {
            Destination = aDestination;
            Source = aSource;
        }

        public Move(string aDestination, UInt32 aSource) {
            Destination = aDestination;
            Source = aSource.ToString();
        }

        public override string ToString() {
			if (String.IsNullOrEmpty(Size)) {
				return "mov " + Destination + "," + Source;
			} else {
				return "mov " + Size + " " + Destination + ", " + Source;
			}
		}
	}
}
