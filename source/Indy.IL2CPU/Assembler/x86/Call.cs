using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "call")]
	public class Call: JumpBase {
		public Call(string aAddress)
			: base(aAddress) {
		}

        protected override bool NeedsNear {
            get {
                return false;
            }
        }
		public override string ToString() {
			return "call " + Address;
		}
	}
}