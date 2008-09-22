using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class PortNumber {
        string mPort;

        public PortNumber(string aPort) {
            mPort = aPort;
        }

        public override string ToString() {
            return mPort;
        }

    }
}
