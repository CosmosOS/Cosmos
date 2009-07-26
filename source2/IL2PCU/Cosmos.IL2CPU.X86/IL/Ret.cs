using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ret)]
	public class Ret: ILOp
	{
		public Ret(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      //TODO: Return
      // Need to jump to end of method. Assembler can emit this label for now
      //new CPU.Jump { DestinationLabel = MethodFooterOp.EndOfMethodLabelNameNormal };
    }
	}
}
