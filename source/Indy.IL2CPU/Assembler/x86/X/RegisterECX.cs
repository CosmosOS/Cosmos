using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterECX : Register32 {
        public const string Name = "ECX";
        public static readonly RegisterECX Instance = new RegisterECX();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterECX(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterECX(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterECX(RegisterEAX aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }
    }
}
