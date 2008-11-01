using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    [OpCode(0xFFFFFFFF, "%define")]
    public class Define: Instruction {
        public string Symbol {
            get;
            set;
        }

        public Define(string aSymbol) {
            Symbol = aSymbol;
        }

        public override string ToString() {
            return "%define " + Symbol + " 1";
        }
    }
}