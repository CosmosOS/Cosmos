using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("out")]
	public class Out: InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xE6 },
                OperandSizeByte=0,
                DestinationImmediate=true,
                DestinationImmediateSize=InstructionSize.Byte

            }); // fixed port (immediate)
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xEE },
                OperandSizeByte = 0,
                DestinationReg=Registers.DX
            }); // fixed port (register)
        }

        public override string ToString() {
            string xReg = "";
            switch(Size) {
                case 8: xReg = "al";break;
                case 16: xReg = "ax"; break;
                case 32: xReg = "eax"; break;

            }
            return Mnemonic + " " + this.GetDestinationAsString() + ", " + xReg;
        }
	}
}
