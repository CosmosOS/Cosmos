using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    public class DataEndIfDefined: DataMember, IEndIfDefined {
        public DataEndIfDefined()
            : base("define", new byte[0]) {
        }

        public override string ToString() {
            return this.GetAsText();
        }
    }
}