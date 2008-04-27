using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterEAX : Register32 {
        public static readonly RegisterEAX Instance = new RegisterEAX();

        protected override string GetName() {
            return "EAX";
        }

        public static implicit operator RegisterEAX(UInt32 aValue) {
            Instance.Move(aValue.ToString());
            return Instance;
        }
    }
}
