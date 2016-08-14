using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86
{
    /// <summary>
    /// Represents the RDMSR-instruction (read model specific register, 0x0f 0x32)
    /// </summary>
    [Cosmos.Assembler.OpCode("rdmsr")]
    public class Rdmsr
        : InstructionWithDestinationAndSourceAndSize
    {
        public Rdmsr() : base("rdmsr")
        {
        }
    }
}
