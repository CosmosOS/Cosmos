using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.x87
{
    [OpCode("fcomip")]
    public class FloatCompareAndSetAndPop : InstructionWithDestination
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDF, 0xF0 },
                DestinationImmediate = false,
                DestinationMemory = true,
                DestinationReg = RegistersEnum.ST0 | RegistersEnum.ST1 | RegistersEnum.ST2 | RegistersEnum.ST3 | RegistersEnum.ST4 | RegistersEnum.ST5 | RegistersEnum.ST6 | RegistersEnum.ST7
            });
        }
    }
}
