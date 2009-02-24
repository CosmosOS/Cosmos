using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public enum ConditionalTestEnum:byte {
        Overflow = 0,
        NoOverflow = 1,
        Below = 2,
        NotAboveOrEqual = 2,
        NotBelow = 3,
        AboveOrEqual = 3,
        Equal = 4,
        Zero = 4,
        NotEqual = 5,
        NotZero =5,
        BelowOrEqual = 6,
        NotAbove = 6,
        NotBelowOrEqual = 7,
        Above = 7,
        Sign = 8,
        NotSign = 9,
        Parity = 10,
        ParityEven = 10,
        NotParity = 11,
        ParityOdd = 11,
        LessThan = 12,
        NotGreaterThanOrEqualTo = 12,
        NotLessThan = 13,
        GreaterThanOrEqualTo = 13,
        LessThanOrEqualTo = 14, 
        NotGreaterThan = 14,
        NotLessThanOrEqualTo = 15,
        GreaterThan = 15
    }
}
