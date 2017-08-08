using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler
{
    public sealed class OpCodeAttribute: Attribute
    {
        public OpCodeAttribute(string mnemonic)
        {
            Mnemonic = mnemonic;
        }

        public string Mnemonic
        {
            get;
            set;
        }
    }
}