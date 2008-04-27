using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public abstract class Register {
        // Not abstract so 8,16,32 dont have to override it
        protected virtual string GetName() {
            throw new NotImplementedException();
        }

        protected void Move(string aValue) {
            new X86.Move(GetName(), aValue);
        }
    }
}
