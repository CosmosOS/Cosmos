using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterESP : Register32 {
        public const string Name = "ESP";
        public static readonly RegisterESP Instance = new RegisterESP();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterESP(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
