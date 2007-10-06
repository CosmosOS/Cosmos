using System;
using System.IO;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public abstract class Instruction {
		protected uint mOpCode;
		protected string mMnemonic;

		public uint OpCode {
			get { return mOpCode; }
		}

		public string Mnemonic {
			get { return mMnemonic; }
		}

		public override string ToString() {
			return Mnemonic;
		}

		public Instruction() {
			var xAttribs = GetType().GetCustomAttributes(typeof(OpCodeAttribute), false);
			if (xAttribs != null && xAttribs.Length > 0) {
				var xAttrib = (OpCodeAttribute)xAttribs[0];
				mOpCode = xAttrib.OpCode;
				mMnemonic = xAttrib.Mnemonic;
			}
		}
	}
}