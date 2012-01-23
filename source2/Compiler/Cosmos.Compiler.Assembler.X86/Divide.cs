using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
	/// <summary>
	/// Puts the result of the divide into EAX, and the remainder in EDX
	/// </summary>
    [Cosmos.Assembler.OpCode("div")]
	public class Divide: InstructionWithDestinationAndSize {
	}
}