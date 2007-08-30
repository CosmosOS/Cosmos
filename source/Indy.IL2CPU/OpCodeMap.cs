using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.IL;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU {
	public class OpCodeMap {
		protected SortedList<Code, Op> mMap = new SortedList<Code, Op>();

		public void LoadOpMapFromAssembly(string aAssemblyName) {
			Assembly xA = Assembly.Load(aAssemblyName);
			foreach (Type t in (from item in xA.GetTypes()
													where item.IsSubclassOf(typeof(Op)) && item.GetCustomAttributes(typeof(OpCodeAttribute), true).Length > 0
													select item)) {
				Op xOp = Activator.CreateInstance(t) as Op;
				object[] xAttribs = t.GetCustomAttributes(typeof(OpCodeAttribute), true);
				try {
					mMap.Add(((OpCodeAttribute)xAttribs[0]).OpCode, xOp);
				} catch (Exception E) {
					Console.WriteLine("Was adding op " + ((OpCodeAttribute)xAttribs[0]).OpCode.ToString());
					throw;
				}
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