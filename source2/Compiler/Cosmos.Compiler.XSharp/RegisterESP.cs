using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
    public class RegisterESP : Register32 {
        public static readonly RegisterESP Instance = new RegisterESP();

        public static implicit operator RegisterESP(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
