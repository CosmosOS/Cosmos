using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Or)]
	public class Or: ILOp
	{
		public Or(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      //TODO: Implement this Op
    }

    
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Or)]
		// 	public class Or: Op {
		// 		public Or(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
		//             new CPUx86.Or { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 			Assembler.StackContents.Pop();
		// 		}
		// 	}
		// }
		
	}
}
