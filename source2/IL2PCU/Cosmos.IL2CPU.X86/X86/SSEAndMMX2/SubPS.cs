using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.SSE
{
    [OpCode("subps")]
    public class SubPS : InstructionWithDestinationAndSource
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x0F, 0x5C }
            });
        }
    }
}
