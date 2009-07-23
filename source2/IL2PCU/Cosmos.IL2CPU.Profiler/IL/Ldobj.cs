using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldobj)]
	public class Ldobj: ILOpProfiler
	{



		#region Old code
		// using System;
		// using Indy.IL2CPU.Assembler.X86;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		//     [Cosmos.IL2CPU.OpCode(ILOp.Code.Ldobj)]
		//     public class Ldobj : ILOpProfiler {
		//         private Type xType;
		// 
		//         public Ldobj(ILReader aReader,
		//                      MethodInformation aMethodInfo)
		//             : base(aReader,
		//                    aMethodInfo) {
		//             xType = aReader.OperandValueType;
		//             if (xType == null) {
		//                 throw new Exception("Type specification not found!");
		//             }
		//         }
		// 
		//         public override void DoAssemble() {
		//             var xSize = GetService<IMetaDataInfoService>().GetFieldStorageSize(xType);
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//             for (int i = 1; i <= (xSize / 4); i++) {
		//                 new CPUx86.Push { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize - (i * 4)) };
		//             }
		//             switch (xSize % 4) {
		//                 case 1: {
		//                         new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
		//                     new CPUx86.Move { DestinationReg = CPUx86.Registers.BL, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
		//                     new CPUx86.Push { DestinationReg = Registers.EBX };
		//                     break;
		//                 }
		//                 case 2: {
		//                         new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
		//                     new CPUx86.Move { DestinationReg = CPUx86.Registers.BX, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
		//                     new CPUx86.Push{DestinationReg=Registers.EBX};
		//                     break;
		//                 }
		//                 case 0: {
		//                     break;
		//                 }
		//                 default: {
		//                     throw new Exception("Remainder not supported!");
		//                 }
		//             }
		//             Assembler.StackContents.Pop();
		//             Assembler.StackContents.Push(new StackContent((int)xSize,
		//                                                           xType));
		//         }
		//     }
		// }
		#endregion
	}
}
