using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class Op: Indy.IL2CPU.IL.Op {

		protected void Call(string aAddress) {
			Assembler.Add(new CPU.Call(aAddress));
		}

		protected void Invoke(string aProcedureName, params object[] aParams) {
			string xResult = "invoke " + aProcedureName;
			foreach (object o in aParams) {
				xResult += ",";
				if (o != null) {
					xResult += o;
				}
			}
			Assembler.Add(new Assembler.Literal(xResult));
		}
	}
}
