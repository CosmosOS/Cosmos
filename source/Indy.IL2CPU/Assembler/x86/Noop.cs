using System.IO;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("nop")]
	public class Noop: Instruction {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0x90 }
            });
        }
	}
}