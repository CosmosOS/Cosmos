using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterEBX : Register32 {
        public const string Name = "EBX";
        public static readonly RegisterEBX Instance = new RegisterEBX();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterEBX(ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
        }

        public static implicit operator RegisterEBX(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterEBX(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterEBX(RegisterECX aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }
    }
}
