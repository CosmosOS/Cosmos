using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("mov")]
	public class Move: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData){
            aData.DefaultSize = false;
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xB0 },
                //NeedsModRMByte=true,
                OperandSizeByte=0,
                OperandSizeBitShiftLeft = 3,
                DestinationReg=Guid.Empty,
                DestinationRegByte=0,
                SourceImmediate=true
            });  // immediate to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xC6 },
                //NeedsModRMByte=true,
                OperandSizeByte = 0,
                OperandSizeBitShiftLeft = 0,
                SourceImmediate = true
            });  // immediate to memory
        }
	}
}
