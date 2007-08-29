using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
	public class OpCodeAttribute: Attribute {
		private readonly Code mOpCode;
		public OpCodeAttribute(Code aOpCode) {
			mOpCode = aOpCode;
		}

		public Code OpCode {
			get {
				return mOpCode;
			}
		}
	}
}