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
				string xPrefix = (from item in Assembler.CurrentInstance.Instructions
								  let xTheLabel = item as Label
								  where xTheLabel != null && !xTheLabel.Name.StartsWith(".")
								  select xTheLabel.Name).Last();
				Address = xPrefix + "__DOT__" + Address.Substring(1);
			}
		}
	}
}