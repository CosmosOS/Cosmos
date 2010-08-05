using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("rol")]
    public class RotateLeft : InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xC0 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xE0,
                OperandSizeByte = 0,
                DestinationRegAny = true,
                ReverseRegisters = true,
                SourceImmediate = true,
                SourceImmediateSize = InstructionSize.Byte
            }); // register by immediate
        }
    }
}