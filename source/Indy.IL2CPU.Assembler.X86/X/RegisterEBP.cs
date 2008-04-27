using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterEBP : Register32 {
        public const string Name = "EBP";
        public static readonly RegisterEBP Instance = new RegisterEBP();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterEBP(UInt32 aValue) {
            Instance.Move(aValue.ToString());
            return Instance;
        }

        // TODO: Use Generics to add all the stuff that is common between all register
        // but must exist on actual register type
        public static implicit operator RegisterEBP(RegisterESP aValue) {
            Instance.Move(aValue.ToString());
            return Instance;
        }
    }
}
