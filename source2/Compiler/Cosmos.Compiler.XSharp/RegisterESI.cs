using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
    public class RegisterESI : Register32 {
        public static readonly RegisterESI Instance = new RegisterESI();

        public static RegisterESI operator ++(RegisterESI aRegister) {
          new Inc { DestinationReg = aRegister.GetId() };
          return aRegister;
        }
        public static RegisterESI operator +(RegisterESI aRegister, UInt32 aValue) {
          Instance.Add(aValue);
          return aRegister;
        }
        public static RegisterESI operator --(RegisterESI aRegister) {
          new Dec { DestinationReg = aRegister.GetId() };
          return aRegister;
        }
        public static RegisterESI operator -(RegisterESI aRegister, UInt32 aValue) {
          Instance.Sub(aValue);
          return aRegister;
        }
        public static RegisterESI operator <<(RegisterESI aRegister, int aCount) {
          new ShiftLeft { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
          return aRegister;
        }
        public static RegisterESI operator >>(RegisterESI aRegister, int aCount) {
          new ShiftRight { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
          return aRegister;
        }

        public static implicit operator RegisterESI(ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
        }

        public static implicit operator RegisterESI(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterESI(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterESI(RegisterESP aValue) {
            Instance.Move(aValue.GetId());
            return Instance;
        }

        public static implicit operator RegisterESI(RegisterEBP aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator RegisterESI(RegisterEAX aValue)
        {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
