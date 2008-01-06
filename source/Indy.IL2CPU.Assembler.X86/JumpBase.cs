using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public abstract class JumpBase: Instruction {
		public readonly string Address;

		protected JumpBase(string aAddress) {
			Address = aAddress;
			if (Address.StartsWith(".")) {
				//string xPrefix = (from item in Assembler.CurrentInstance.Instructions
				//                  where !Label.GetLabel(item).StartsWith(".")
				//                  select Label.GetLabel(item)).Last();
				string xPrefix = Label.LastFullLabel;
				Address = xPrefix + "__DOT__" + Address.Substring(1);
			}
		}
	}
}