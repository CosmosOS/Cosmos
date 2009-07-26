using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Dup)]
	public class Dup: ILOp
	{
		public Dup(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      var xStackContent = OldAsmblr.StackContents.Peek();
      for (int i = 0; i < ((xStackContent.Size / 4) + (xStackContent.Size % 4 == 0 ? 0 : 1)); i++) {
        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceDisplacement = i * 4, SourceIsIndirect = true };
        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
      }
      OldAsmblr.StackContents.Push(xStackContent);
    }
		
	}
}
