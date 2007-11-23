using System;
using System.Collections.Generic;
using System.Text;

namespace Indy.IL2CPU.Plugs {
	public abstract class AssemblerMethod {
		public abstract void Assemble(Assembler.Assembler aAssembler);
	}
}