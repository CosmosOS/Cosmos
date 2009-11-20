using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.x87
{
    [OpCode("fclex")]
    public class FloatClearEx : Instruction
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x9B, 0xDB, 0xE2 }
            });
        }
    }
}