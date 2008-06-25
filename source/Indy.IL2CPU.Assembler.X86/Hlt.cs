using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode(0xF4, "hlt")]
    public class Hlt : Instruction
    {
    }
}