using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.X {
    public class Register32 : Register {

        // Not all overloads can go here.
        // 1- C# overloads specifically by exact class and does not inherit in many cases
        // 2- x86 does not support all operations on all registers

        public static AddressIndirect operator+ (Register32 aBaseRegister, Int32 aDisplacement) {
            return new AddressIndirect(aBaseRegister, aDisplacement);
        }

        public void Add(UInt32 aValue) {
            new Add { DestinationReg = GetId(), SourceValue = aValue };
        }
        public void Add(Register32 aReg) {
            new Add { DestinationReg = GetId(), SourceReg = aReg.GetId() };
        }

        public void Sub(UInt32 aValue) {
            new Sub { DestinationReg = GetId(), SourceValue = aValue };
        }
        public void Sub(Register32 aReg) {
            new Sub { DestinationReg = GetId(), SourceReg = aReg.GetId() };
        }

        public void Compare(UInt32 aValue) {
            new Compare { DestinationReg = GetId(), SourceValue = aValue };
        }
        public void Compare(MemoryAction aAction) {
            new Compare {
                DestinationRef = aAction.Reference,
                DestinationIsIndirect = true,
                SourceReg = GetId()
            };
        }

        public void Test(UInt32 aValue) {
            new Test { DestinationReg = GetId(), SourceValue = aValue, Size = 32 };
        }
    }
}