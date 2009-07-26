using System;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.ILOpCodes;
using CPU = Indy.IL2CPU.Assembler.X86;
using System.Collections.Generic;
using StackContent = Indy.IL2CPU.Assembler.StackContent;

namespace Cosmos.IL2CPU.X86.IL {
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldc_I4)]
	public class Ldc_I4: ILOp {
		public Ldc_I4(ILOpCode aOpCode):base(aOpCode) {}

    public override void Execute(uint aMethodUID) {
			new CPU.Push { DestinationValue = ((OpInt)OpCode).Value };
      Asmblr.StackContents.Push(new StackContent(4, typeof(int)));
    }

	}
}

