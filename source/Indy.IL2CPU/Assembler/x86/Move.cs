using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("mov")]
	public class Move: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData){
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xC6 },
                NeedsModRMByte=true,
                OperandSizeByte=0,
                DestinationReg=Guid.Empty,
                DestinationRegByte=1,
                SourceImmediate=true
            });
        }
	}
}
