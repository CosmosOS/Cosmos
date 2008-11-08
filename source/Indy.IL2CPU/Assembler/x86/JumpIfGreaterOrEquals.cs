using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[Obsolete("This instruction is a copy of JumpIfGreaterOrEqual, and has wrong opcode in attribute")]
    [OpCode("jge")]
	public class JumpIfGreaterOrEquals: JumpBase {
	}
}