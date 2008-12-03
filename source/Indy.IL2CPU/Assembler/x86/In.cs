using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("in")]
    public class In : InstructionWithDestinationAndSource {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xE4 },
                OperandSizeByte = 0,
                DestinationReg=Registers.EAX,
                SourceImmediate = true,
                SourceImmediateSize = InstructionSize.Byte,
                DefaultSize=InstructionSize.DWord
            }); // fixed port (immediate)
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xEC },
                OperandSizeByte = 0,
                SourceReg = Registers.DX,
                DefaultSize = InstructionSize.DWord,
                DestinationReg=Registers.EAX
            }); // fixed port (register)
        }
    }
}
