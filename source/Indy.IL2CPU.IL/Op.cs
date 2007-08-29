using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	public abstract class Op {
		public abstract byte OpCode();
		public abstract void Process(ILReader aReader);
	}
}
