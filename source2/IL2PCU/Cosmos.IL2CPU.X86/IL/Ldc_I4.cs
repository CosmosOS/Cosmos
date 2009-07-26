using System;
using Cosmos.IL2CPU.ILOpCodes;
using Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler.X86;
using System.Collections.Generic;

namespace Cosmos.IL2CPU.X86.IL {
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldc_I4)]
	public class Ldc_I4: ILOpX86 {
		public Ldc_I4(ILOpCode aOpCode):base(aOpCode) { }

    public override void Execute(uint aMethodUID) {
      new CPU.Push { DestinationValue = ((OpInt)OpCode).Value };
      ((CPU.Assembler)Assembler.CurrentInstance.Peek()).StackContents.Push(new StackContent(4, typeof(int)));
    }

	}
}
