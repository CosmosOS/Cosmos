using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterAX : Register16 {
        public const string Name = "AX";
        public static readonly RegisterAX Instance = new RegisterAX();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterAX(ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
        }

        public static implicit operator RegisterAX(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterAX(UInt16 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator PortNumber(RegisterAX aAX)
        {
            return new PortNumber(aAX.GetId());
        }

    }
}
