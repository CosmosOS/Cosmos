using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JMP opcode
	/// </summary>
    [OpCode("jmp")]
	public class Jump: JumpBase {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xE9 },
                DestinationImmediate = true,
                AllowedSizes=InstructionSizes.DWord
            }); // direct to immediate
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFF },
                DestinationMemory = true,
                AllowedSizes = InstructionSizes.DWord,
                ReverseRegisters = true,
                NeedsModRMByte=true,
                InitialModRMByteValue = 0x20
            }); // indirect at memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFF },
                DestinationReg = Guid.Empty,
                AllowedSizes = InstructionSizes.DWord,
                ReverseRegisters=true,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xE0
            }); // indirect at register
        }
	}
}