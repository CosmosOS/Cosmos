using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class InstructionWithDestinationAndSource : InstructionWithDestination {
        public ElementReference SourceRef {
            get;
            set;
        }

        public Guid SourceReg {
            get;
            set;
        }

        public uint SourceValue {
            get;
            set;
        }

        public bool SourceIsIndirect {
            get;
            set;
        }

        public int SourceDisplacement {
            get;
            set;
        }

        protected string GetSourceAsString() {
            string xDest = "";
            if (SourceRef != null) {
                xDest = SourceRef.ToString();
            } else {
                if (SourceReg != Guid.Empty) {
                    xDest = Registers.GetRegisterName(SourceReg);
                } else {
                    xDest = "0x" + SourceValue.ToString("X").ToUpperInvariant();
                }
            }
            if (SourceDisplacement != 0) {
                xDest += " + " + SourceDisplacement;
            }
            if (SourceIsIndirect) {
                return "[" + xDest + "]";
            } else {
                return xDest;
            }
        }

        public override string ToString() {
            return Mnemonic + " " + GetDestinationAsString() + ", " + GetSourceAsString();
        }
    }
}