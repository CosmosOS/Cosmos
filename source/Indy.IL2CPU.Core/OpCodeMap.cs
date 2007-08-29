using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU {
	public class OpCodeMap {
		protected SortedList<Code, Op> mMap = new SortedList<Code, Op>();

		public OpCodeMap() {
			foreach (Type t in (from item in typeof(Op).Assembly.GetTypes()
													where item.IsSubclassOf(typeof(Op)) && item.GetCustomAttributes(typeof(OpCodeAttribute), false).Length > 0
													select item)) {
				Op xOp = Activator.CreateInstance(t) as Op;
				object[] xAttribs = t.GetCustomAttributes(typeof(OpCodeAttribute), false);
				mMap.Add(((OpCodeAttribute)xAttribs[0]).OpCode, xOp);
			}
		}

		public Op GetOpForOpCode(Code code) {
			if (!mMap.ContainsKey(code)) {
				throw new NotSupportedException("OpCode '" + code + "' not supported!");
			}
			return mMap[code];
		}
	}
}