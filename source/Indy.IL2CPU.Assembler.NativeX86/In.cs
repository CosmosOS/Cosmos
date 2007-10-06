using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.NativeX86 {
	[OpCode(0xFFFFFFFF, "in")]
	public class In: Instruction {
		public readonly string Port;
		public readonly string Data;
		public In(string aPort, string aData) {
			Port = aPort;
			Data = aData;
		}

		public override string ToString() {
			return "in " + Data + ", " + Port;
		}
	}
}