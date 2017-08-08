using System;

using CPU = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Volatile)]
	public class Volatile: ILOp
	{
		public Volatile(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode) {
      //TODO: Volatile
    }

	}
}
