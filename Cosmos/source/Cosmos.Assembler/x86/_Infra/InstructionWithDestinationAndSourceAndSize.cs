using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86
{
    public abstract class InstructionWithDestinationAndSourceAndSize : InstructionWithDestinationAndSource,
        IInstructionWithSize
    {
        // todo: do all instructions with two operands have a size?
        //todo should validate Instructions or use a constructor and no set properties. 
        protected byte mSize;

        public InstructionWithDestinationAndSourceAndSize()
        {

        }

        public InstructionWithDestinationAndSourceAndSize(string mnemonic) : base(mnemonic)
        {

        }

        //Size in bits
        public byte Size
        {
            get
            {
                DetermineSize();
                return mSize;
            }
            set
            {
                if (value == 0 || value % 8 != 0)
                {
                    throw new ArgumentException(
                        string.Format("Size must be greater than 0 and bits in byte sizes. Your Value: {0:D}", value));
                }
                mSize = value;

                //ben will crash on output for size 0 so catch here
            }
        }

        protected virtual void DetermineSize()
        {
            if (mSize == 0)
            {
                if (DestinationReg != null && !DestinationIsIndirect)
                {
                    Size = Registers.GetSize(DestinationReg.Value);
                    return;
                }
                if (SourceReg != null && !SourceIsIndirect)
                {
                    Size = Registers.GetSize(SourceReg.Value);
                    return;
                }
                if ((SourceRef != null && !SourceIsIndirect) || (DestinationRef != null && !DestinationIsIndirect))
                {
                    Size = 32;
                    return;
                }
            }

        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            if (Size == 0)
            {
                Size = 32;
                //Console.WriteLine("ERRROR no size set for Instruction - set to 4 InstructionWithDestinationAndSourceAndSize") ;
            }
            aOutput.Write(mMnemonic);

            if (!DestinationEmpty)
            {
                if (DestinationIsIndirect)
                {
                    aOutput.Write(" ");
                    aOutput.Write(SizeToString(Size));
                }

                aOutput.Write(" ");
                aOutput.Write(this.GetDestinationAsString());
                aOutput.Write(", ");

                if (SourceIsIndirect)
                {
                    aOutput.Write(SizeToString(Size));
                    aOutput.Write(" ");
                }

                aOutput.Write(GetSourceAsString());
            }
        }
    }
}