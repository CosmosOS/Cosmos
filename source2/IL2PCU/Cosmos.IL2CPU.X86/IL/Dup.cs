using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Dup)]
	public class Dup: ILOpX86
	{



		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Dup)]
		// 	public class Dup: ILOpX86 {
		// 		public Dup(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			var xStackContent = Assembler.StackContents.Peek();
		// 			for (int i = 0; i < ((xStackContent.Size / 4) + (xStackContent.Size % 4 == 0 ? 0 : 1)) ; i++) {
		//                 new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceDisplacement=i*4, SourceIsIndirect=true };
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 			}
		// 			Assembler.StackContents.Push(xStackContent);
		// 		}
		// 	}
		// }
		#endregion
	}
}
