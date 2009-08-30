using System;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Throw)]
	public class Throw: ILOp
	{
		public Throw(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      throw new NotImplementedException();
    }

		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Throw)]
		// 	public class Throw: Op {
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
	}
}
