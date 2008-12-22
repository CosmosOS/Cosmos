using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode("aam")]
    public class AAM : Instruction
    {
        public static void InitializeEncodingData(Instruction.InstructionData aData)
        {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption
            {
                OpCode = new byte[] { 0xD4, 0x0A }
            });
        }
    }
}
