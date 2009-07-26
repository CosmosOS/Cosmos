using System;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Unbox_Any)]
	public class Unbox_Any: ILOp
	{
		public Unbox_Any(ILOpCode aOpCode):base(aOpCode)
		{
		}

    public override void Execute(uint aMethodUID) {
      base.Execute(aMethodUID);
    }

  }
}
