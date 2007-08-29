using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	public class Ldstr: Op {
		public override byte OpCode() {
			return 0x72;
		}

		public override void Process(ILReader aReader) {
		}

	}
}