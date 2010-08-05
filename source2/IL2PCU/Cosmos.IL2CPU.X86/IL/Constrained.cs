using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Constrained)]
	public class Constrained: ILOp
	{
		public Constrained(Cosmos.Compiler.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      // todo: Implement correct Constrained support
        throw new NotImplementedException();
    }

	}
}
