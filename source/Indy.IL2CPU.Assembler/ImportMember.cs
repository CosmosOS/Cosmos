using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	public class ImportMember {
		public readonly string Data;
		public ImportMember(string aData) {
			Data = aData;
		}

		public override string ToString() {
			return Data;
		}
	}
}
