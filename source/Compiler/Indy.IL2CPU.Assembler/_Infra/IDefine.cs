using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    public interface IDefine {
        string Symbol {
            get;
            set;
        }
    }
}
