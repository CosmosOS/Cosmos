using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public interface IInstructionWithDestination {
        ElementReference DestinationRef {
            get;
            set;
        }

        Guid DestinationReg {
            get;
            set;
        }

        uint? DestinationValue {
            get;
            set;
        }

        bool DestinationIsIndirect {
            get;
            set;
        }

        int DestinationDisplacement {
            get;
            set;
        }
    }
}
