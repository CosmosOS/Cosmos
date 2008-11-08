using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Subtracts the source operand from the destination operand and 
	/// replaces the destination operand with the result. 
	/// </summary>
    [OpCode("sub")]
	public class Sub: InstructionWithDestinationAndSourceAndSize {
	}
}