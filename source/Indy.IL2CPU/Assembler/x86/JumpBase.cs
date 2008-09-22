using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public abstract class JumpBase: Instruction {
		public readonly string Address;
        protected virtual bool NeedsNear{get{return true;}}
		protected JumpBase(string aAddress) {
			if (aAddress.StartsWith(".")) {
				aAddress = Label.LastFullLabel + "__DOT__" + aAddress.Substring(1);
			}
			// If it has a :, then its a far call so dont add near
			if (aAddress.Contains(':') || !NeedsNear) {
			    Address = aAddress;
			} else {
			    // Nasm by default issues conditional branches as short
			    // and we often exceed the distance
			    // For now we go for simplicity. Later when we optimize and have
			    // our own assembler, we should consider using short jumps
			    // if they are faster
			    Address = "near " + aAddress;
			}
		}
	}
}