using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    public class IfNotDefined: Instruction, IIfNotDefined {
        public string Symbol {
            get;
            set;
        }

        public override string ToString() {
            return this.GetAsText();
        }
    }
}