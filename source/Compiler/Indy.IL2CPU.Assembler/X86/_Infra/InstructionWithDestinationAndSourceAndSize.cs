using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithDestinationAndSourceAndSize : InstructionWithDestinationAndSource, IInstructionWithSize {
        // todo: do all instructions with two operands have a size?
        //todo should validate Instructions or use a constructor and no set properties. 
        protected byte mSize;
        
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
                    throw new ArgumentException("Size must be greater than 0 and bits in byte sizes");  

              mSize = value;

              //ben will crash on output for size 0 so catch here
                
            }

            
        }

        protected virtual void DetermineSize() {
            if (mSize == 0) {
                if (DestinationReg != null && !DestinationIsIndirect) {
                    if (Registers.Is16Bit(DestinationReg.Value)) {
                        Size = 16;
                    } else {
                        if (Registers.Is32Bit(DestinationReg.Value)) {
                            Size = 32;
                        } else {
                            Size = 8;
                        }
                    }
                    return;
                }
                if (SourceReg != null && !SourceIsIndirect) {
                    if (Registers.Is16Bit(SourceReg.Value)) {
                        Size = 16;
                    } else {
                        if (Registers.Is32Bit(SourceReg.Value)) {
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

        public override void WriteText(Indy.IL2CPU.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            if (Size == 0)
            {
                Size = 32;
                Console.WriteLine("ERRROR no size set for Instruction - set to 4 InstructionWithDestinationAndSourceAndSize") ;
            }
            aOutput.Write(mMnemonic);
            aOutput.Write(" ");
            aOutput.Write(SizeToString(Size));
            aOutput.Write(" ");
            aOutput.Write(this.GetDestinationAsString());
            aOutput.Write(", ");
            aOutput.Write(this.GetSourceAsString());
        }
    }
}