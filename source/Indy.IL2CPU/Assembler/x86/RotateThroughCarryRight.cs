using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("rcr")]
    public class RotateThroughCarryRight : InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xD2, 0xD8 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 1,
                OperandSizeByte = 0,
                SourceReg = Registers.CL,
                SourceRegByte = 0,
                SourceRegBitShiftLeft = 6,
            }); // register by CL
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xD2 },
                NeedsModRMByte=true,
                InitialModRMByteValue = 0x18,
                DestinationMemory = true,
                OperandSizeByte = 0,
                SourceReg = Registers.CL,
                SourceRegByte = -1,
                ReverseRegisters=true
            }); // memory by CL
        }
    }
}