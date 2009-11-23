using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.x87
{
    [OpCode("fisub")]
    public class IntSub : InstructionWithDestinationAndSize
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDA },
                NeedsModRMByte = true,
                InitialModRMByteValue = 4,
                DestinationImmediate = false,
                DestinationMemory = true,
                DestinationReg = null,
                AllowedSizes = InstructionSizes.DWord,
                DefaultSize = InstructionSize.DWord
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDE },
                NeedsModRMByte = true,
                InitialModRMByteValue = 4,
                DestinationImmediate = false,
                DestinationMemory = true,
                DestinationReg = null,
                AllowedSizes = InstructionSizes.Word,
                DefaultSize = InstructionSize.Word
            });
        }
    }
}