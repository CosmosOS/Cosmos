using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
    class Noop : Op {
        public override int OpCode() {
            return 0;
        }
    }
}
