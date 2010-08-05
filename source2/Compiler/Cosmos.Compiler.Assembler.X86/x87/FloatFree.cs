using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.x87
{
    [OpCode("ffree")]
    public class FloatFree : InstructionWithDestination
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDD, 0xC0 },
                SourceReg=null,
                SourceImmediate=false,
                SourceMemory=false,
                DestinationImmediate=false,
                DestinationMemory=false,
                DestinationReg = RegistersEnum.ST0 | RegistersEnum.ST1 | RegistersEnum.ST2 | RegistersEnum.ST3 | RegistersEnum.ST4 | RegistersEnum.ST5 | RegistersEnum.ST6 | RegistersEnum.ST7
            });
        }
    }
}