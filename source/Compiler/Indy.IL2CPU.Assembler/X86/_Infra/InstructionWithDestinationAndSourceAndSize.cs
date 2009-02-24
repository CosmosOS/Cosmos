using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithDestinationAndSourceAndSize : InstructionWithDestinationAndSource, IInstructionWithSize {
        // todo: do all instructions with two operands have a size?
        protected byte mSize;
        public byte Size {
            get{
                DetermineSize();
                return mSize;}
            set {
                if (value > 0) {
                    SizeToString(value);
                }
                mSize = value;
            }
        }

        protected virtual void DetermineSize() {
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
                if (SourceReg != Guid.Empty && !SourceIsIndirect) {
                    if (Registers.Is16Bit(SourceReg)) {
                        Size = 16;
                    } else {
                        if (Registers.Is32Bit(SourceReg)) {
                            Size = 32;
                        } else {
                            Size = 8;
                        }
                    }
                    return;
                }
                if ((SourceRef != null && !SourceIsIndirect) || (DestinationRef != null && !DestinationIsIndirect)) {
                    Size = 32;
                    return;
                }
            }
        }

        public override string ToString() {
            return base.mMnemonic + " " + SizeToString(Size) + " " + this.GetDestinationAsString() + ", " + GetSourceAsString();
        }
    }
}