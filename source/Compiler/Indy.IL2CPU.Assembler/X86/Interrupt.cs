using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("int")]
    public class Interrupt : InstructionWithDestination {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xCD },
                DestinationImmediate=true,
                DestinationImmediateSize=InstructionSize.Byte
            });
        }
        public override string ToString() {
            //if (Number == INTO) return "into";
            return "int " + DestinationValue;
        }
    }
}