using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("mov")]
    public class MoveCR : InstructionWithDestinationAndSource {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xC0 },
                DestinationReg = Registers.CR0,
                DestinationRegByte = -1,
                SourceReg = Guid.Empty,
                SourceRegByte = 2
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xD0 },
                DestinationReg = Registers.CR2,
                DestinationRegByte = -1,
                SourceReg = Guid.Empty,
                SourceRegByte = 2
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xD8 },
                DestinationReg = Registers.CR3,
                DestinationRegByte = -1,
                SourceReg = Guid.Empty,
                SourceRegByte = 2
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xE0 },
                DestinationReg = Registers.CR4,
                DestinationRegByte = -1,
                SourceReg = Guid.Empty,
                SourceRegByte = 2
            });

            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x20, 0xC0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft = 0,
                SourceReg = Registers.CR0,
                SourceRegByte = 2,
                SourceRegBitShiftLeft = 3
            }); // CR to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x20, 0xC0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft = 0,
                SourceReg = Registers.CR2,
                SourceRegByte = 2,
                SourceRegBitShiftLeft = 3
            }); // CR to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x20, 0xC0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft = 0,
                SourceReg = Registers.CR3,
                SourceRegByte = 2,
                SourceRegBitShiftLeft = 3
            }); // CR to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x20, 0xC0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft = 0,
                SourceReg = Registers.CR4,
                SourceRegByte = 2,
                SourceRegBitShiftLeft = 3
            }); // CR to register
        }
    }
}