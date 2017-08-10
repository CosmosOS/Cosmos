using System;


namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Endfilter)]
	public class Endfilter: ILOp
	{
		public Endfilter(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode) {
      throw new NotImplementedException();
    }

	}
}
