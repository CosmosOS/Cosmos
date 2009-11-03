using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.SSE
{
    [OpCode("cmpss")]
    public class CompareSS : InstructionWithDestinationAndSourceAndPseudoOpcodes
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xF3,0x0F,0xC2 }
            });
        }
    }
}
