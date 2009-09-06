using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
	/// <summary>
	/// Puts the result of the divide into EAX, and the remainder in EDX
	/// </summary>
    [OpCode("idiv")]
	public class IDivide: InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xF6, 0xF8 },
                OperandSizeByte = 0,
                ReverseRegisters = true,
                DestinationRegAny = true,
                DestinationRegByte=1
            });// register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xF6 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x38,
                ReverseRegisters = true,
                OperandSizeByte = 0,
                DestinationMemory = true
            }); // memory
        }
	}
}