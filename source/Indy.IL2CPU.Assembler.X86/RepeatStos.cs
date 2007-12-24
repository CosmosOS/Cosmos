using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "rep stosb")]
	public class RepeatStos: Instruction {
		//public readonly string Destination;
		public RepeatStos() {
			//	Destination = aDestination;
		}
		//public override string ToString() {
		//    return "rep stos " + Destinati;
		//}
	}
}
