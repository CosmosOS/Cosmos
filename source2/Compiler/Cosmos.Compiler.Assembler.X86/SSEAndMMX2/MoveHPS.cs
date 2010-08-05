using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.SSE
{
    [OpCode("movhps")]
    public class MoveHPS : InstructionWithDestinationAndSource
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x0F, 0x16 },
                NeedsModRMByte = true,
                DestinationImmediate = false,
                SourceImmediate = false,
                DestinationMemory = false,
                SourceMemory = true,
                DestinationReg = RegistersEnum.XMM0 | RegistersEnum.XMM1 | RegistersEnum.XMM2 | RegistersEnum.XMM3 | RegistersEnum.XMM4 | RegistersEnum.XMM5 | RegistersEnum.XMM6 | RegistersEnum.XMM7,
                InitialModRMByteValue = 0x08,
                ReverseRegisters = true
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x0F, 0x17 },
                NeedsModRMByte = true,
                DestinationImmediate = false,
                SourceImmediate = false,
                DestinationMemory = true,
                SourceMemory = false,
                InitialModRMByteValue = 0x08,
                SourceReg = RegistersEnum.XMM0 | RegistersEnum.XMM1 | RegistersEnum.XMM2 | RegistersEnum.XMM3 | RegistersEnum.XMM4 | RegistersEnum.XMM5 | RegistersEnum.XMM6 | RegistersEnum.XMM7,
                ReverseRegisters = true
            });
        }
    }
}
