using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86
{
    [OpCode("movd")]
    public class MoveD : InstructionWithDestinationAndSource
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x0F, 0x6E },
                SourceImmediate=false,
                DestinationImmediate=false,
                SourceMemory=true,
                DestinationMemory=false,
                NeedsModRMByte=true,
                InitialModRMByteValue=0x80

            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x0F, 0x7E },
                SourceImmediate=false,
                DestinationImmediate=false,
                SourceMemory = false,
                DestinationMemory = true,
                NeedsModRMByte=true,
                InitialModRMByteValue=0x80
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x66,0x0F, 0x6E },
                SourceImmediate = false,
                DestinationImmediate = false,
                SourceMemory = true,
                DestinationMemory = false,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x80,
                DestinationReg = Registers.XMM0 | Registers.XMM1 | Registers.XMM2 | Registers.XMM3 | Registers.XMM4 | Registers.XMM5 | Registers.XMM6 | Registers.XMM7
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x66, 0x0F, 0x7E },
                SourceImmediate = false,
                DestinationImmediate = false,
                SourceMemory = false,
                DestinationMemory = true,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x80,
                SourceReg = Registers.XMM0 | Registers.XMM1 | Registers.XMM2 | Registers.XMM3 | Registers.XMM4 | Registers.XMM5 | Registers.XMM6 | Registers.XMM7
            });
        }
    }
}
