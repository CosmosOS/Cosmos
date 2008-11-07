using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithDestinationAndSource: InstructionWithDestination {
        // todo: do all instructions with two operands have a size?
        private byte mSize;
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

        protected string GetSourceAsString() {
            string xDest = "";
            if (SourceRef != null) {
                xDest = SourceRef.ToString();
            } else {
                if(SourceReg != Guid.Empty) {
                    xDest = Registers.GetRegisterName(SourceReg);
                }else {
                    xDest = "0x" + SourceValue.ToString("X").ToUpperInvariant();
                }
            }
            if(SourceDisplacement!= 0) {
                xDest += " + " + SourceDisplacement;
            }
            if(SourceIsIndirect) {
                return "[" + xDest + "]";
            }else {
                return xDest;
            }
        }
    }
}