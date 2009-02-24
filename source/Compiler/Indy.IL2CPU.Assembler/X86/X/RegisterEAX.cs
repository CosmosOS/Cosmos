using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterEAX : Register32 {
        public const string Name = "EAX";
        public static readonly RegisterEAX Instance = new RegisterEAX();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterEAX(ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
        }

        public static implicit operator RegisterEAX(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterEAX(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterEAX(RegisterECX aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }
        public static implicit operator PortNumber(RegisterEAX aEAX)
        {
            return new PortNumber(aEAX.GetId());
        }
    }
}
