using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterESI : Register32 {
        public const string Name = "ESI";
        public static readonly RegisterESI Instance = new RegisterESI();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterESI(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterESI(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterESI(RegisterESP aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }

        public static implicit operator RegisterESI(RegisterEBP aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
