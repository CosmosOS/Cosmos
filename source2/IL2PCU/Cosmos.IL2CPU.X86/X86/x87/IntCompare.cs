using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.x87
{
    [OpCode("ficom")]
    public class IntCompare : InstructionWithDestination
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDE },
                NeedsModRMByte = true,
                InitialModRMByteValue = 2,
                DestinationImmediate = false,
                DestinationMemory = true,
                DestinationReg = null,
                SourceImmediate = false,
                SourceReg = null,
                SourceMemory = false
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xDA },
                NeedsModRMByte = true,
                InitialModRMByteValue = 2,
                DestinationImmediate = false,
                DestinationMemory = true,
                DestinationReg = null,
                SourceImmediate = false,
                SourceReg = null,
                SourceMemory = false
            });
        }
    }
}