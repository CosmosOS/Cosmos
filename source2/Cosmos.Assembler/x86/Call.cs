using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    [Cosmos.Assembler.OpCode("call")]
	public class Call: JumpBase {
        public Call():base("call") {
            mNear = false;
        }
	}
}