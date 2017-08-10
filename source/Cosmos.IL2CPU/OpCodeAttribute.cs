using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class OpCodeAttribute : Attribute
    {
		public ILOpCode.Code OpCode
		{
			get
			{
				return opCode;
			}
		}
        private readonly ILOpCode.Code opCode;

        public OpCodeAttribute(ILOpCode.Code OpCode)
        {
            this.opCode = OpCode;
        }

        public string Mnemonic
        {
            get
            {
                return mnemonic;
            }
        }

        //OLD:
        private readonly string mnemonic;

        public OpCodeAttribute(string Mnemonic)
        {
            this.mnemonic = Mnemonic;
        }
    }

}
