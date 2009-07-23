using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.And)]
	public class And: ILOpProfiler
	{



		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.And)]
		// 	public class And: ILOpProfiler {
		// 		public And(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			StackContent xStackContent = Assembler.StackContents.Peek();
		// 			if (xStackContent.IsFloat) {
		// 				throw new Exception("Floats not yet supported!");
		// 			}
		// 			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
		// 			if (xSize > 8) {
		// 				throw new Exception("StackSize>8 not supported");
		// 			}
		// 			if (xSize > 4)
		// 			{
		//                 new CPU.Pop { DestinationReg = CPU.Registers.EAX };
		//                 new CPU.Pop { DestinationReg = CPU.Registers.EBX };
		//                 new CPU.Pop { DestinationReg = CPU.Registers.EDX };
		//                 new CPU.Pop { DestinationReg = CPU.Registers.ECX };
		//                 new CPU.And { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.EDX };
		//                 new CPU.And { DestinationReg = CPU.Registers.EBX, SourceReg = CPU.Registers.ECX };
		//                 new CPU.Push { DestinationReg = CPU.Registers.EBX };
		//                 new CPU.Push { DestinationReg = CPU.Registers.EAX };
		// 			}else
		// 			{
		//                 new CPU.Pop { DestinationReg = CPU.Registers.EAX };
		//                 new CPU.Pop { DestinationReg = CPU.Registers.EDX };
		//                 new CPU.And { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.EDX }; 
		//                 new CPU.Push { DestinationReg = CPU.Registers.EAX };
		// 			}
		// 			Assembler.StackContents.Push(xStackContent);
		// 		}
		// 	}
		// }
		#endregion
	}
}
