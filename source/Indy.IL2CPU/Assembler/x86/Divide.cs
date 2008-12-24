using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Puts the result of the divide into EAX, and the remainder in EDX
	/// </summary>
    [OpCode("div")]
	public class Divide: InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xF6 },
                NeedsModRMByte=true,
                InitialModRMByteValue = 0xF0,
                ReverseRegisters=true,
                OperandSizeByte=0,
                DestinationReg=Guid.Empty
            });// register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] {0xF6},
                NeedsModRMByte=true,
                InitialModRMByteValue = 0x30,
                ReverseRegisters = true,
                OperandSizeByte = 0,
                DestinationMemory=true
            }); // memory
        }
	}
}