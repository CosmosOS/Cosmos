using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Mul_Ovf)]
	public class Mul_Ovf: ILOp
	{
		public Mul_Ovf(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      throw new NotImplementedException();
    }

    
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Cosmos.Assembler.x86;
		// 
		// namespace Cosmos.IL2CPU.IL.X86 {
		// 	[Cosmos.Assembler.OpCode(OpCodeEnum.Mul_Ovf)]
		// 	public class Mul_Ovf: Mul {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Mul_Ovf(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 
		// 		public override void DoAssemble()
		// 		{
		//             EmitNotImplementedException(Assembler, GetServiceProvider(), "Mul_Ovf not yet implemented", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		// 		}
		// 	}
		// }
		
	}
}
