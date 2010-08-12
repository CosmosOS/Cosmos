using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
    public class RegisterAL : Register08 {
        public static readonly RegisterAL Instance = new RegisterAL();

        public static implicit operator RegisterAL(byte aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterAL(char aValue) {
            Instance.Move((byte)aValue);
            return Instance;
        }

        public static implicit operator RegisterAL(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterAL(PortNumber aPort) {
            new In { DestinationReg = Registers.AL };
            return Instance;
        }

        public static implicit operator PortNumber(RegisterAL aAL) {
            return new PortNumber(aAL.GetId());
        }

    }
}
