using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware2 {
    public enum EFlagsEnum : uint {
        Carry = 1,
        Parity = 1 << 2,
        AuxilliaryCarry = 1 << 4,
        Zero = 1 << 6,
        Sign = 1 << 7,
        Trap = 1 << 8,
        InterruptEnable = 1 << 9,
        Direction = 1 << 10,
        Overflow = 1 << 11,
        NestedTag = 1 << 14,
        Resume = 1 << 16,
        Virtual8086Mode = 1 << 17,
        AlignmentCheck = 1 << 18,
        VirtualInterrupt = 1 << 19,
        VirtualInterruptPending = 1 << 20,
        ID = 1 << 21
    }
}