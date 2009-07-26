using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Add)]
	public class Add : ILOp
	{
		public Add(ILOpCode aOpCode)
			: base(aOpCode)
		{
		}

		public override void Execute(uint aMethodUID)
		{
			Add(Asmblr);
		}
	}
}
