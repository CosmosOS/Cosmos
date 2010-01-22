using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.X {
    public class RegisterEDI : Register32 {
        public const string Name = "EDI";
        public static readonly RegisterEDI Instance = new RegisterEDI();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterEDI(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterEDI(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterEDI(RegisterESP aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }

        public static implicit operator RegisterEDI(RegisterEBP aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
