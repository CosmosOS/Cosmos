using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    [OpCode("%ifdef")]
    public class IfDefined: Instruction, IIfDefined {
        public string Symbol {
            get;
            set;
        }

        public IfDefined(string aSymbol) {
            Symbol = aSymbol;
        }

        public override string ToString() {
            return this.GetAsText();
        }
    }
}