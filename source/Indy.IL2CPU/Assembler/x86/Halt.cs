using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode(0xF4, "hlt")]
    public class Halt : Instruction
    {
        public override bool DetermineSize(Indy.IL2CPU.Assembler.Assembler aAssembler, out ulong aSize) {
            aSize = 1;
            return true;
        }

        public override bool IsComplete(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            return true;
        }

        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            return new byte[] { 0xF4 };
        }
    }
}