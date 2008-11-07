using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public abstract class JumpBase: InstructionWithDestination {
        public override string ToString() {
            // always use near for now
            if (Mnemonic == "call") {
                return Mnemonic + " " + GetDestinationAsString();
            }
            return Mnemonic + " near " + GetDestinationAsString();
        }

        public string DestinationLabel {
            get {
                if (DestinationRef != null) {
                    return DestinationRef.Name;
                }
                return String.Empty;
            }
            set {
                DestinationRef = new ElementReference(value);
            }
        }
	}
}