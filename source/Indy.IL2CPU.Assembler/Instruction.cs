using System;
using System.IO;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public abstract class Instruction {
		protected Instruction() {
			Assembler.Current.Add(this);
		}

		public virtual void EmitParams(BinaryWriter aWriter) {
		}
	}
}