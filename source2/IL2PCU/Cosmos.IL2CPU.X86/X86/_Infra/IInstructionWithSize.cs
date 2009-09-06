using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
    public interface IInstructionWithSize {
        byte Size {
            get;
            set;
        }
    }
}