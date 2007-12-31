using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "rep movsb")]
	public class RepeatMovsd: Instruction {
		//public readonly string Destination;
		public RepeatMovsd() {
			//	Destination = aDestination;
		}
		//public override string ToString() {
		//    return "rep stos " + Destinati;
		//}
	}
}
