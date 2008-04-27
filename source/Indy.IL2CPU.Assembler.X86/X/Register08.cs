using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Register08 : Register {
        public void Test(byte aValue) {
            new X86.Test(GetName(), aValue);
        }
    }
}
