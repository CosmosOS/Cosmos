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

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      new CPU.Jump { DestinationLabel = "_" + aMethod.UID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value };
    }
		
	}
}
