using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [Cosmos.Assembler.OpCode("call")]
	public class Call: JumpBase {
        public Call() {
            mNear = false;
        }
	}
}