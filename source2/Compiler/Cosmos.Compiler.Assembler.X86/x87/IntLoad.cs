using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.x87
{
    [OpCode("fild")]
    public class IntLoad : InstructionWithDestinationAndSize
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDF },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0,
                DestinationImmediate=false,
                DestinationReg=null,
                AllowedSizes=InstructionSizes.Word,
                DefaultSize = InstructionSize.Word
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDB },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0,
                DestinationMemory = true,
                DestinationImmediate = false,
                DestinationReg = null,
                AllowedSizes = InstructionSizes.DWord,
                DefaultSize = InstructionSize.DWord
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDF },
                NeedsModRMByte=true,
                InitialModRMByteValue=5,
                DestinationMemory = true,
                DestinationImmediate = false,
                DestinationReg = null,
                AllowedSizes = InstructionSizes.QWord,
                DefaultSize = InstructionSize.QWord
            });
        }
    }
}
