using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("mov")]
	public class Move: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xC0 },
                DestinationReg = Registers.CR0,
                //DestinationRegByte = -1,
                SourceReg = Guid.Empty,
                SourceRegByte = 2
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xD0 },
                DestinationReg = Registers.CR2,
                //DestinationRegByte = -1,
                SourceReg = Guid.Empty,
                SourceRegByte = 2
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xD8 },
                DestinationReg = Registers.CR3,
                //DestinationRegByte = -1,
                SourceReg = Guid.Empty,
                SourceRegByte = 2
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xE0 },
                DestinationReg = Registers.CR4,
                //DestinationRegByte = -1,
                SourceReg = Guid.Empty,
                SourceRegByte = 2
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xE0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft=0,
                SourceReg = Registers.CR0,
                SourceRegByte = 2,
                SourceRegBitShiftLeft=3
            }); // CR to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xE0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft = 0,
                SourceReg = Registers.CR1,
                SourceRegByte = 2,
                SourceRegBitShiftLeft = 3
            }); // CR to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xE0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft = 0,
                SourceReg = Registers.CR2,
                SourceRegByte = 2,
                SourceRegBitShiftLeft = 3
            }); // CR to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xE0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft = 0,
                SourceReg = Registers.CR3,
                SourceRegByte = 2,
                SourceRegBitShiftLeft = 3
            }); // CR to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x22, 0xE0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 2,
                DestinationRegBitShiftLeft = 0,
                SourceReg = Registers.CR4,
                SourceRegByte = 2,
                SourceRegBitShiftLeft = 3
            }); // CR to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xB0 },
                //NeedsModRMByte=true,
                DefaultSize = InstructionSize.DWord,
                AllowedSizes = InstructionSizes.Byte | InstructionSizes.Word | InstructionSizes.DWord,
                OperandSizeByte = 0,
                OperandSizeBitShiftLeft = 3,
                DestinationReg = Guid.Empty,
                DestinationRegByte = 0,
                SourceImmediate = true
            });  // immediate to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xC6 },
                NeedsModRMByte = true,
                //AllowedSizes = InstructionSizes.Word,
                OperandSizeByte = 0,
                OperandSizeBitShiftLeft = 0,
                SourceImmediate = true,
                //DestinationRegByte = 1,
                ReverseRegisters=true,
                DestinationMemory = true
            });  // immediate to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x88  },
                OperandSizeByte = 0,
                NeedsModRMByte=true,
                ReverseRegisters=true,
                InitialModRMByteValue=0xC0,
                SourceReg=Guid.Empty,
                DestinationReg=Guid.Empty
            }); // register to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xA2 },
                OperandSizeByte = 0,
                SourceReg = Registers.EAX,
                DestinationMemory = true,
                DestinationMemoryKinds = OperandMemoryKinds.Address,
                DestinationImmediateSize=InstructionSize.DWord,
                ReverseRegisters=false
            }); // register to memory (eax)
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x88 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                ReverseRegisters=true,
                SourceReg=Guid.Empty,
                DestinationMemory = true
            }); // register to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xA0 },
                OperandSizeByte = 0,
                SourceMemory = true,
                SourceMemoryKinds = OperandMemoryKinds.Address,
                DestinationReg = Registers.EAX,
                SourceImmediateSize = InstructionSize.DWord
            }); // memory to register (eax)
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x8A },
                OperandSizeByte=0,
                NeedsModRMByte = true,
                SourceMemory=true,
                DestinationReg=Guid.Empty
            }); // memory to register
        }
	}
}
