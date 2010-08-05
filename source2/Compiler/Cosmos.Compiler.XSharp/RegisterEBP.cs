using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.X {
    public class RegisterEBP : Register32 {
        public static readonly RegisterEBP Instance = new RegisterEBP();

        public static implicit operator RegisterEBP(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterEBP(RegisterESP aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }
    }
}
