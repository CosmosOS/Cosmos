using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("rep movsb")]
	public class RepeatMovsb: Instruction {
		//public readonly string Destination;
		public RepeatMovsb() {
			//	Destination = aDestination;
		}
		//public override string ToString() {
		//    return "rep stos " + Destinati;
		//}
	}
}
