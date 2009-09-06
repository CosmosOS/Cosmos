using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
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
        public override void WriteText( Cosmos.IL2CPU.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            base.WriteText(aAssembler, aOutput);
            aOutput.Write(", DX");
        }
    }
}
