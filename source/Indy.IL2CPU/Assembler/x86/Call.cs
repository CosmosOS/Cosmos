using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("call")]
	public class Call: JumpBase {
        public Call() {
            mNear = false;
        }
	}
}