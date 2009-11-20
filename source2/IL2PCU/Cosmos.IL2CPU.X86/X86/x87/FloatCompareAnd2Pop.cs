using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.X86.x87
{
    [OpCode("fcompp")]
    public class FloatCompareAnd2Pop : Instruction
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
                {
                    OpCode = new byte[] { 0xDE, 0xD9 },
                });
        }
    }
}
