using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Assembler.XSharp {
    public class RegisterBL : Register08 {
        public static readonly RegisterBL Instance = new RegisterBL();

        public static implicit operator RegisterBL(byte aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterBL(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterBL(RegisterAL aReg) {
            Instance.Move(aReg.GetId());
            return Instance;
        }

        public static implicit operator RegisterBL(PortNumber aPort) {
            new IN { DestinationReg = Registers.BL };
            return Instance;
        }

    }
}
