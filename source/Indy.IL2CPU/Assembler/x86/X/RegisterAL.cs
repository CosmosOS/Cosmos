using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterAL : Register08 {
        public const string Name = "AL";
        public static readonly RegisterAL Instance = new RegisterAL();

        public override string ToString() {
            return Name;
        }

        // TODO: Use an attribute to find the register name
        // Also useful for Memory conversion - Can find attribute
        // of descendant? or no?
        public static implicit operator RegisterAL(byte aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterAL(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterAL(PortNumber aPort) {
            new X86.In { DestinationReg = Registers.AL };
            return Instance;
        }

        public static implicit operator PortNumber(RegisterAL aAL) {
            return new PortNumber(aAL.GetId());
        }

    }
}
