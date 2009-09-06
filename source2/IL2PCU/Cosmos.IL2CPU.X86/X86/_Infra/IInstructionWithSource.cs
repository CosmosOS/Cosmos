using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
    public interface IInstructionWithSource {
        ElementReference SourceRef {
            get;
            set;
        }

        RegistersEnum? SourceReg
        {
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