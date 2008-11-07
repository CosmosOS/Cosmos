using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "pop")]
	public class Pop: InstructionWithDestinationAndSize {
		public override string ToString() {
            if (Size > 0) {
                return "pop " + SizeToString(Size) + " " + GetDestinationAsString();
            }
            return "pop dword " + GetDestinationAsString();
		} 
	}
}