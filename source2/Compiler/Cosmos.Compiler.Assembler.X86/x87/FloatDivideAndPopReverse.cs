using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.x87
{
    [OpCode("fdivpr")]
    public class FloatDivideAndPopReverse : InstructionWithDestinationAndSource
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDE, 0xF1 },
                DestinationEmpty=true,
                SourceEmpty=true

            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDE, 0xF0 },
                SourceReg = RegistersEnum.ST0,
                DestinationReg = RegistersEnum.ST0 | RegistersEnum.ST1 | RegistersEnum.ST2 | RegistersEnum.ST3 | RegistersEnum.ST4 | RegistersEnum.ST5 | RegistersEnum.ST6 | RegistersEnum.ST7,
                SourceImmediate = false,
                SourceMemory = false,
                DestinationMemory = false,
                DestinationImmediate = false
            });
        }
    }
}