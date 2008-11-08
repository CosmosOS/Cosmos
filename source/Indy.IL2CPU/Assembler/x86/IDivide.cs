using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Puts the result of the divide into EAX, and the remainder in EDX
	/// </summary>
	[OpCode(0xFFFFFFFF, "idiv")]
	public class IDivide: InstructionWithDestinationAndSize {
	}
}
