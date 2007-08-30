using System;
using System.IO;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public abstract class Instruction {
        // For threading, later on we can use TLS or add a paramterized constructor
		protected Instruction() {
			Assembler.Current.Add(this);
		}
	}
}