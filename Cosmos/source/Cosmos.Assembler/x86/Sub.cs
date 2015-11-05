using System;
using System.Linq;

namespace Cosmos.Assembler.x86 {
    /// <summary>
    /// Subtracts the source operand from the destination operand and 
    /// replaces the destination operand with the result. 
    /// </summary>
    [Cosmos.Assembler.OpCode("sub")]
    public class Sub : InstructionWithDestinationAndSourceAndSize {
    }
}