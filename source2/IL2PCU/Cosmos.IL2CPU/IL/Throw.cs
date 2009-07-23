using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Throw)]
	public class Throw: ILOpCode
	{



		#region Old code
		// using System;
		// using System.IO;
		// using System.Linq;
		// using Indy.IL2CPU.Compiler;
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Throw)]
		// 	public class Throw: ILOpCode {
		// 		private MethodInformation mMethodInfo;
		// 		private int mCurrentILOffset;
		//         public Throw(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mMethodInfo = aMethodInfo;
		// 			mCurrentILOffset = (int)aReader.Position;
		// 		}
		// 
		// 		public static void Assemble(Assembler.Assembler aAssembler, MethodInformation aMethodInfo, int aCurrentILOffset, string aExceptionOccurredLabel) {
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//             new CPUx86.Move { DestinationRef = CPU.ElementReference.New(CPU.DataMember.GetStaticFieldName(CPU.Assembler.CurrentExceptionRef)), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
		//             new CPUx86.Call { DestinationLabel = aExceptionOccurredLabel };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 3 };
		// 			Call.EmitExceptionLogic(aAssembler, (uint)aCurrentILOffset, aMethodInfo, null, false, null);
		// 			aAssembler.StackContents.Pop();
		// 		}
		// 	
		// 		public override void DoAssemble() {
		// 		    var xMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(CPU.Assembler.CurrentExceptionOccurredRef,
		// 		                                                                       false);
		// 			Assemble(Assembler, mMethodInfo, mCurrentILOffset, xMethodInfo.LabelName);
		// 		}
		// 	}
		// }
		#endregion
	}
}
