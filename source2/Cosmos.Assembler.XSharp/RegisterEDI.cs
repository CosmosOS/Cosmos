using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Assembler.XSharp {
    public class RegisterEDI : Register32 {
        public static readonly RegisterEDI Instance = new RegisterEDI();

        public static RegisterEDI operator ++(RegisterEDI aRegister) {
            new INC { DestinationReg = aRegister.GetId() };
            return aRegister;
        }
        public static RegisterEDI operator --(RegisterEDI aRegister) {
            new Dec { DestinationReg = aRegister.GetId() };
            return aRegister;
        }
        public static RegisterEDI operator <<(RegisterEDI aRegister, int aCount) {
            new ShiftLeft { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
            return aRegister;
        }
        public static RegisterEDI operator >>(RegisterEDI aRegister, int aCount) {
            new ShiftRight { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
            return aRegister;
        }
        
        public static implicit operator RegisterEDI(Cosmos.Assembler.ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
        }

        public static implicit operator RegisterEDI(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterEDI(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterEDI(RegisterEAX aValue) {
          Instance.Move(aValue);
          return Instance;
        }
        public static implicit operator RegisterEDI(RegisterESP aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }

        public static implicit operator RegisterEDI(RegisterEBP aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
