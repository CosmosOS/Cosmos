using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.Native {
    [OpCode(0xFFFFFFFF, "inw")]
    public class InDWord : Instruction {
        public readonly string Port;
        public readonly string Data;
        public InDWord(string aData, string aPort) {
            Port = aPort;
            Data = aData;
        }

        public override string ToString() {
            return "in dword " + Data + ", " + Port;
        }
    }
}