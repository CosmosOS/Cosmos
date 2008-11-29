using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("jmp")]
    public class JumpToSegment: Instruction {
        public ElementReference DestinationRef {
            get;
            set;
        }

        public ushort Segment {
            get;
            set;
        }

        public override string ToString() {
            return "jmp " + Segment + ":" + DestinationRef.ToString();
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