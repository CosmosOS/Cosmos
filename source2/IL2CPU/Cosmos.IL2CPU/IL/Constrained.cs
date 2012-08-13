using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Constrained)]
	public class Constrained: ILOp
	{
		public Constrained(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
			#warning TODO: This needs to either be implemented, or needs to throw a NotImplementedException!
      // todo: Implement correct Constrained support
        //throw new NotImplementedException("Constrained used in '" + aMethod.MethodBase.GetFullName() + "'");
    }

	}
}
