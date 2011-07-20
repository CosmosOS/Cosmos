using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;

namespace Cosmos.Compiler.XSharp {
    public class Address {

        public static implicit operator Address(string aLabel) {
          return new AddressDirect(aLabel);
        }

    }
}
