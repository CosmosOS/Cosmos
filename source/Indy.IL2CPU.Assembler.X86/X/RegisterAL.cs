using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterAL : Register08 {
        public static readonly RegisterAL Instance = new RegisterAL();

        protected override string GetName() {
            return "AL";
        }

        public static implicit operator RegisterAL(byte aValue) {
            Instance.Move(aValue.ToString());
            return Instance;
        }

        public static implicit operator RegisterAL(PortSource aValue) {
            new X86.InByte(Instance.GetName(), aValue.ToString());
            return Instance;
        }

    }
}
