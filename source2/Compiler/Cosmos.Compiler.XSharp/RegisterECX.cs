using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.X {
    public class RegisterECX : Register32 {
        public static readonly RegisterECX Instance = new RegisterECX();

        public static implicit operator RegisterECX(ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
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

        public static implicit operator RegisterECX(RegisterEBX aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }

        public static implicit operator RegisterECX(RegisterEDX aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }
    }
}
