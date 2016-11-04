using System;

namespace Cosmos.IL2CPU.IL.x86
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Jmp)]
	public class Jmp: ILOp
	{
		public Jmp(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      throw new NotImplementedException();
    }

	}
}
