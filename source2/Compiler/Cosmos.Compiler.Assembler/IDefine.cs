using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler {
    public interface IDefine {
        string Symbol {
            get;
            set;
        }
    }
}
