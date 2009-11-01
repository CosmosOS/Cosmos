using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.SSE
{
    [OpCode("movss")]
    public class MoveSS : InstructionWithDestinationAndSource
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xF3, 0x0F, 0x10 }
            });

            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xF3, 0x0F, 0x11 }
            });
        }
    }
}
