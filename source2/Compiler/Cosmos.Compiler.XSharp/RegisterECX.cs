using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
  public class RegisterECX : Register32 {
    public static readonly RegisterECX Instance = new RegisterECX();

    public static RegisterECX operator ++(RegisterECX aRegister) {
      new Inc { DestinationReg = aRegister.GetId() };
      return aRegister;
    }
    public static RegisterECX operator --(RegisterECX aRegister) {
      new Dec { DestinationReg = aRegister.GetId() };
      return aRegister;
    }
    public static RegisterECX operator <<(RegisterECX aRegister, int aCount) {
      new ShiftLeft { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
      return aRegister;
    }
    public static RegisterECX operator >>(RegisterECX aRegister, int aCount) {
      new ShiftRight { DestinationReg = aRegister.GetId(), SourceValue = (uint)aCount };
      return aRegister;
    }

    public static implicit operator RegisterECX(ElementReference aReference) {
      Instance.Move(aReference);
      return Instance;
    }

    public static implicit operator RegisterECX(MemoryAction aAction) {
      Instance.Move(aAction);
      return Instance;
    }

    public static implicit operator RegisterECX(UInt32 aValue) {
      Instance.Move(aValue);
      return Instance;
    }

    public static implicit operator RegisterECX(RegisterEAX aValue) {
      Instance.Move(aValue.GetId());
      return Instance;
    }

    public static implicit operator RegisterECX(RegisterEBX aValue) {
      Instance.Move(aValue.GetId());
      return Instance;
    }

    public static implicit operator RegisterECX(RegisterEDX aValue) {
      Instance.Move(aValue.GetId());
      return Instance;
    }
  }
}
