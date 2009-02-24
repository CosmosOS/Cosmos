using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterEDX : Register32 {
        public const string Name = "EDX";
        public static readonly RegisterEDX Instance = new RegisterEDX();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterEDX(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterEDX(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
