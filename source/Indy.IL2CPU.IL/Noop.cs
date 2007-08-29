using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	class Noop: Op {
		public override byte OpCode() {
			return 0;
		}

		public override void Process(ILReader aReader) {
			Console.WriteLine("NoOp encountered");
		}
	}
}
