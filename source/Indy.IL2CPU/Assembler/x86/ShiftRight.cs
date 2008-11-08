using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("shr")]
	public class ShiftRight: InstructionWithDestination {
        public byte? Count {
            get;
            set;
        }

		public override string ToString() {
            if (Count.HasValue) {
                return "shr " + this.GetDestinationAsString() + ", " + Count.Value.ToString();
            } else {
                return "shr " + this.GetDestinationAsString() + ", CL";
            }
		}
	}
}