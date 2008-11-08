using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class InstructionWithDestinationAndSize : InstructionWithDestination {
        private void DetermineSize() {
            if (mSize == 0) {
                if (DestinationReg != Guid.Empty && !DestinationIsIndirect) {
                    if (Registers.Is16Bit(DestinationReg)) {
                        Size = 16;
                    } else {
                        if (Registers.Is32Bit(DestinationReg)) {
                            Size = 32;
                        } else {
                            Size = 8;
                        }
                    }
                    return;
                }
                if (DestinationRef != null && !DestinationIsIndirect) {
                    Size = 32;
                    return;
                }
            }
        }
        private byte mSize;
        public byte Size {
            get {
                DetermineSize();
                return mSize;
            }
            set {
                if (value > 0) {
                    SizeToString(value);
                }
                mSize = value;
            }
        }

        public override string ToString() {
            return base.mMnemonic + " " + SizeToString(Size) + " " + GetDestinationAsString();
        }
    }
}
