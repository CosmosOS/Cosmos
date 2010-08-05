using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Arglist)]
	public class Arglist: ILOp
	{
		public Arglist(Cosmos.Compiler.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

		public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
		{
			throw new NotImplementedException("Arglist not yet implemented!");
		}
		
	}
}
