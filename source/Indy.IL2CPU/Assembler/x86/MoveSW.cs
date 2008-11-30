using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode("movsw")]
    public class MoveSW: Instruction
    {
#warning todo:merge with MoveSB, and inherit from InstructionWithSize
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x66, 0xA5 }
            });
        }
    }
}
