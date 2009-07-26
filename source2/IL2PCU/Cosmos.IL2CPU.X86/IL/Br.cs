using System;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Br)]
	public class Br: ILOp
	{
		public Br(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      new CPU.Jump { DestinationLabel = "_" + aMethodUID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value };
    }
		
	}
}
