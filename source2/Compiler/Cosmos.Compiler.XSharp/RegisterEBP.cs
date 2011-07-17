using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
  public class RegisterEBP : Register32 {
    public static readonly RegisterEBP Instance = new RegisterEBP();

    public static RegisterEBP operator ++(RegisterEBP aRegister) {
      new Inc { DestinationReg = aRegister.GetId() };
      return aRegister;
    }
    public static RegisterEBP operator +(RegisterEBP aRegister, UInt32 aValue) {
      Instance.Add(aValue);
      return aRegister;
    }
    public static RegisterEBP operator --(RegisterEBP aRegister) {
      new Dec { DestinationReg = aRegister.GetId() };
      return aRegister;
    }
    public static RegisterEBP operator -(RegisterEBP aRegister, UInt32 aValue) {
      Instance.Sub(aValue);
      return aRegister;
    }
    public static RegisterEBP operator <<(RegisterEBP aRegister, int aCount) {
      new ShiftLeft { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
      return aRegister;
    }
    public static RegisterEBP operator >>(RegisterEBP aRegister, int aCount) {
      new ShiftRight { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
      return aRegister;
    }

    public static implicit operator RegisterEBP(UInt32 aValue) {
      Instance.Move(aValue);
      return Instance;
    }

    public static implicit operator RegisterEBP(RegisterESP aValue) {
      Instance.Move(aValue.GetId());
      return Instance;
    }
  }
}
