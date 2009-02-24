using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("in")]
    public class In : InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xEC },
                OperandSizeByte=0,
                DefaultSize = InstructionSize.Byte,
                DestinationReg=Registers.AL
            }); // fixed port (register)
        }

        public override string ToString() {
            return base.ToString() + ", DX";
        }
    }
}
