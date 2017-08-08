using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Arglist)]
	public class Arglist: ILOp
	{
		public Arglist(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

		public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
		{
			throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Arglist.cs->Error: Arglist not yet implemented!");
		}

	}
}
