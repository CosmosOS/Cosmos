using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithDestinationAndSize : InstructionWithDestination, IInstructionWithSize {
        private byte mSize;
        public byte Size {
            get {
                this.DetermineSize(this, mSize);
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
            return base.mMnemonic + " " + SizeToString(Size) + " " + this.GetDestinationAsString();
        }
    }
}
