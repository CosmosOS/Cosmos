using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.XSharp {
  public class RegisterEDX : Register32 {
    public static readonly RegisterEDX Instance = new RegisterEDX();

    public static implicit operator RegisterEDX(MemoryAction aAction) {
      Instance.Move(aAction);
      return Instance;
    }

    public static implicit operator RegisterEDX(UInt32 aValue) {
      Instance.Move(aValue);
      return Instance;
    }
  }
}
