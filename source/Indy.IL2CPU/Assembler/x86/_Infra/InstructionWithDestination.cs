using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithDestination : Instruction, IInstructionWithDestination {
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

        public override string ToString() {
            return base.mMnemonic + " " + this.GetDestinationAsString();
        }
    }
}