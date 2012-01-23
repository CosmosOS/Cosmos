using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;

namespace Cosmos.Assembler.XSharp {
    public class Address {

        public static implicit operator Address(string aLabel) {
          return new AddressDirect(aLabel);
        }

    }
}
