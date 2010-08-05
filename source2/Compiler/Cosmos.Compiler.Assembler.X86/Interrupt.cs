using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("int")]
    public class Interrupt : InstructionWithDestination {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xCD },
                DestinationImmediate=true,
                DestinationImmediateSize=InstructionSize.Byte
            });
        }
        public override void WriteText( Cosmos.Compiler.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            aOutput.Write("int ");
            aOutput.Write(DestinationValue);
        }
    }
}