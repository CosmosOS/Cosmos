using System;


namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Break)]
	public class Break: ILOp
	{
		public Break(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}
		
		public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
		{
			throw new Exception("Cosmos.IL2CPU.x86->IL->Break.cs->Error: Break op-code has not been implemented yet!");
		}

	}
}
