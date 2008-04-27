using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public abstract class Register {

        public void Push() {
            new Push(ToString());
        }

        protected void Move(string aValue) {
            new X86.Move(ToString(), aValue);
        }

    }
}
