using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
  public class AddressDirect : Address {
    public readonly string Label;
    public readonly uint Address;
    public readonly RegistersEnum? Register;

    public AddressDirect(RegistersEnum aRegister) {
      Register = aRegister;
    }

    public AddressDirect(UInt32 aAddress) {
      Address = aAddress;
    }

    public AddressDirect(string aLabel) {
      Label = aLabel;
    }

    public override string ToString() {
      if (Label == null) {
        return Address.ToString();
      } else if (Register != null) {
        return Registers.GetRegisterName(Register.Value);
      }
      return Label;
    }

  }
}
