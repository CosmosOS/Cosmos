using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithDestination : New_Instruction {
        public ElementReference DestinationRef {
            get;
            set;
        }

        public Guid DestinationReg {
            get;
            set;
        }

        public uint DestinationValue {
            get;
            set;
        }

        public bool DestinationIsIndirect {
            get;
            set;
        }

        public int DestinationDisplacement {
            get;
            set;
        }

        protected string GetDestinationAsString() {
            string xDest = "";
            if (DestinationRef != null) {
                xDest = DestinationRef.ToString();
            } else {
                if (DestinationReg != Guid.Empty) {
                    xDest = Registers.GetRegisterName(DestinationReg);
                } else {
                    xDest = "0x" + DestinationValue.ToString("X").ToUpperInvariant();
                }
            }
            if (DestinationDisplacement != 0) {
                xDest += " + " + DestinationDisplacement;
            }
            if (DestinationIsIndirect) {
                return "[" + xDest + "]";
            } else {
                return xDest;
            }
        }
    }
}