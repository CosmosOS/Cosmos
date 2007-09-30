using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public abstract class Assembler: Indy.IL2CPU.Assembler.Assembler {
		public Assembler(StreamWriter aOutputWriter)
			: base(aOutputWriter) {
		}

		public Assembler(StreamWriter aOutputWriter, bool aInMetalMode)
			: base(aOutputWriter, aInMetalMode) {
		}
	}
}
