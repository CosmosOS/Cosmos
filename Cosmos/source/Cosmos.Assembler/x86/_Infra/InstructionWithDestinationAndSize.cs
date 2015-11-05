using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    public abstract class InstructionWithDestinationAndSize : InstructionWithDestination, IInstructionWithSize {
        public InstructionWithDestinationAndSize(string mnemonic = null) : base(mnemonic)
        {
        }

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

        public override void WriteText( Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
{
            aOutput.Write(mMnemonic);
            aOutput.Write(" ");
            aOutput.Write(SizeToString(Size));
            if (!DestinationEmpty)
            {
                aOutput.Write(" ");
                aOutput.Write(this.GetDestinationAsString());
            }
        }
    }
}
