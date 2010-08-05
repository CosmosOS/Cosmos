using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.X {
    public class RegisterESI : Register32 {
        public static readonly RegisterESI Instance = new RegisterESI();

        public static implicit operator RegisterESI(ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
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
