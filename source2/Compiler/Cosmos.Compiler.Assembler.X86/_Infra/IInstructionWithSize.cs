using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    public interface IInstructionWithSize {
        byte Size {
            get;
            set;
        }
    }
}