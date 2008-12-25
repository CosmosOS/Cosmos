using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("out")]
    public class Out : InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xEE },
                OperandSizeByte=0,
                DestinationReg=Registers.AL,
                DefaultSize = InstructionSize.Byte
            }); // fixed port (register)
        }

        public override string ToString() {
            return base.mMnemonic + " DX, " + this.GetDestinationAsString();
        }
	}
}
