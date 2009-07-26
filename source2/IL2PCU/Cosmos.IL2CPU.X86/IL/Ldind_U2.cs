using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldind_U2)]
	public class Ldind_U2: ILOp
	{
		public Ldind_U2(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      //TODO: Implement this Op
    }

    
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldind_U2)]
		// 	public class Ldind_U2: Op {
		// 		public Ldind_U2(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			Assembler.StackContents.Pop();
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceValue = 0 };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.DX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect=true };
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
		// 			Assembler.StackContents.Push(new StackContent(2, typeof(ushort)));
		// 		}
		// 	}
		// }
		
	}
}
