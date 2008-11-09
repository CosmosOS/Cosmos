using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public abstract class JumpBase: InstructionWithDestination {
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

        public override string ToString() {
            var xResult = base.ToString();
            if (!xResult.StartsWith(Mnemonic + " near", StringComparison.InvariantCultureIgnoreCase)) {
                if (xResult.StartsWith(Mnemonic)) {
                    return Mnemonic + " near " + xResult.Substring(Mnemonic.Length + 1);
                }
            }
            return xResult;
        }
	}
}