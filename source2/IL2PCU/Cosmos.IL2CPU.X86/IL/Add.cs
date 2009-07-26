using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Add)]
	public class Add : ILOp
	{
		public Add(Cosmos.IL2CPU.Assembler aAsmblr)
			: base(aAsmblr)
		{
		}

		public override void Execute(uint aMethodUID, ILOpCode aOpCode)
		{
      throw new Exception("TODO:");
			//Add(Asmblr);
		}
	}
}
