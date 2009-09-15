using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using Cosmos.IL2CPU;

namespace Indy.IL2CPU.X86.PlugsLinqTest.CustomImplementations.System.Diagnostics {
	public class BreakAssembler: AssemblerMethod {
    public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			//new Assembler.X86.Call(Assembler.X86.Assembler.BreakMethodName);
			// TODO - See Cosmos.Debug.Debugger.Break - this should use same plug
            // TODO -pHigh: implement Debugger.Break();
		}

    public override void AssembleNew(object aAssembler, object aMethodInfo) {
    }
	}
}