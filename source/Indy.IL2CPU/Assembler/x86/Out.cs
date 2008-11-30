using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("out")]
	public class Out: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xE6 },
                OperandSizeByte=0,
                DestinationImmediate=true,
                DestinationImmediateSize=InstructionSize.Byte,
                SourceReg=Registers.EAX,
                DefaultSize=InstructionSize.DWord
            }); // fixed port (immediate)
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xEE },
                OperandSizeByte = 0,
                DestinationReg=Registers.DX,
                SourceReg = Registers.EAX,
                DefaultSize=InstructionSize.DWord
            }); // fixed port (register)
        }
	}
}
