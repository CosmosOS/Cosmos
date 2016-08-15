using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86
{
    /// <summary>
    /// Represents the RDTSC-instruction (read timestamp counter, 0x0f 0x31)
    /// </summary>
    [Cosmos.Assembler.OpCode("rdtsc")]
    public class Rdtsc
        : InstructionWithDestinationAndSourceAndSize
    {
        public Rdtsc() : base("rdtsc")
        {
        }
    }
}
