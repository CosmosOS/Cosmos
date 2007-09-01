using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
    public abstract class Op : Indy.IL2CPU.IL.Op {

        public void Call(string aAddress) {
            Assembler.Add(new CPU.Call(aAddress));
		}

    }
}
