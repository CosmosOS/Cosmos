using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL;

namespace Indy.IL2CPU {
	public class OpCodeMap {
		protected SortedList<byte, Op> mMap = new SortedList<byte, Op>();

		public OpCodeMap() {
			foreach (Type t in (from item in typeof(Op).Assembly.GetTypes()
													where item.IsSubclassOf(typeof(Op))
													select item)) {
				Op op = Activator.CreateInstance(t) as Op;
				mMap.Add(op.OpCode(), op);
			}
		}

		public Op GetOpForOpCode(byte code)
		{
			if (!mMap.ContainsKey(code)) {
				throw new NotSupportedException("OpCode '" + code.ToString("X2") + "' not supported!");
			}
			return mMap[code];
		}
	}
}