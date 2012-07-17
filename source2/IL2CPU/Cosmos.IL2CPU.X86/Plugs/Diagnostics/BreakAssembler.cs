using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Assembler;

namespace Cosmos.IL2CPU.X86.PlugsLinqTest.CustomImplementations.System.Diagnostics {
	public class BreakAssembler: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
    }
	}
}