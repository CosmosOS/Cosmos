using System;

namespace Cosmos.Assembler
{
    public sealed class OpCodeAttribute : Attribute
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