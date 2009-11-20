using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.x87
{
    [OpCode("fcom")]
    public class FloatCompare : InstructionWithDestinationAndSize
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xD8 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 2,
                DestinationImmediate = false,
                DestinationMemory = true,
                DestinationReg = null,
                SourceImmediate = false,
                SourceReg = null,
                SourceMemory = false,
                AllowedSizes=InstructionSizes.DWord,
                DefaultSize=InstructionSize.DWord
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDC },
                NeedsModRMByte = true,
                InitialModRMByteValue = 2,
                DestinationImmediate = false,
                DestinationMemory = true,
                DestinationReg = null,
                SourceImmediate = false,
                SourceReg = null,
                SourceMemory = false,
                AllowedSizes = InstructionSizes.QWord,
                DefaultSize = InstructionSize.QWord
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xD8, 0xD0 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 2,
                DestinationImmediate = false,
                DestinationMemory = false,
                DestinationReg = RegistersEnum.ST0 | RegistersEnum.ST1 | RegistersEnum.ST2 | RegistersEnum.ST3 | RegistersEnum.ST4 | RegistersEnum.ST5 | RegistersEnum.ST6 | RegistersEnum.ST7,
                SourceImmediate = false,
                SourceReg = null,
                SourceMemory = false
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xD8, 0xD1 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 2,
                DestinationImmediate = false,
                DestinationMemory = false,
                DestinationReg = null,
                SourceImmediate = false,
                SourceReg = null,
                SourceMemory = false
            });
        }
    }
}