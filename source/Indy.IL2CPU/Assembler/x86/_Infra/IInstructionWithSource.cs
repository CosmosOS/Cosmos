using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public interface IInstructionWithSource {
        ElementReference SourceRef {
            get;
            set;
        }

        Guid SourceReg {
            get;
            set;
        }

        uint? SourceValue {
            get;
            set;
        }

        bool SourceIsIndirect {
            get;
            set;
        }

        int SourceDisplacement {
            get;
            set;
        }
    }
}