using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldind_R8)]
	public class Ldind_R8: ILOpProfiler
	{



		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldind_R8)]
		// 	public class Ldind_R8: ILOpProfiler {
		// 		public Ldind_R8(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
		// 			Assembler.StackContents.Pop();
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4 };
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true};
		// 			Assembler.StackContents.Push(new StackContent(8, typeof(Double)));
		// 		}
		// 	}
		// }
		#endregion
	}
}
