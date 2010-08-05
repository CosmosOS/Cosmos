using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("call")]
	public class Call: JumpBase {
        public Call() {
            mNear = false;
        }
	}
}