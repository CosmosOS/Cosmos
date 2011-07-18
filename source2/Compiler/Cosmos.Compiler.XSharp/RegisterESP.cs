using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
    public class RegisterESP : Register32 {
        public static readonly RegisterESP Instance = new RegisterESP();

        public static RegisterESP operator ++(RegisterESP aRegister) {
          new INC { DestinationReg = aRegister.GetId() };
          return aRegister;
        }
        public static RegisterESP operator +(RegisterESP aRegister, UInt32 aValue) {
          Instance.Add(aValue);
          return aRegister;
        }
        public static RegisterESP operator --(RegisterESP aRegister) {
          new Dec { DestinationReg = aRegister.GetId() };
          return aRegister;
        }
        public static RegisterESP operator -(RegisterESP aRegister, UInt32 aValue) {
          Instance.Sub(aValue);
          return aRegister;
        }
        public static RegisterESP operator <<(RegisterESP aRegister, int aCount) {
          new ShiftLeft { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
          return aRegister;
        }
        public static RegisterESP operator >>(RegisterESP aRegister, int aCount) {
          new ShiftRight { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
          return aRegister;
        }

        public static implicit operator RegisterESP(UInt32 aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
