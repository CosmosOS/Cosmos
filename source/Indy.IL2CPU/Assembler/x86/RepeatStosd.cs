using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("rep stosd")]
	public class RepeatStosd: Instruction {
		//public readonly string Destination;
		public RepeatStosd() {
			//	Destination = aDestination;
		}
		//public override string ToString() {
		//    return "rep stosddword ";
		//}
	}
}
