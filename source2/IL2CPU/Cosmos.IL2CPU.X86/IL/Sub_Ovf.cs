using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Sub_Ovf)]
	public class Sub_Ovf: ILOp
	{
		public Sub_Ovf(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      throw new NotImplementedException();
    }

    
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Cosmos.Compiler.Assembler.X86;
		// 
		// namespace Cosmos.IL2CPU.IL.X86 {
		// 	[Cosmos.Assembler.OpCode(OpCodeEnum.Sub_Ovf)]
		// 	public class Sub_Ovf: Op {
		// 		private string mCurrentLabel;
		// 	    private string mNextLabel;
		// 	    private uint mCurrentOffset;
		// 	    private MethodInformation mCurrentMethodInfo;
		//         public Sub_Ovf(ILReader aReader, MethodInformation aMethodInfo)
		//             : base(aReader, aMethodInfo)
		//         {
		//             mCurrentLabel = GetInstructionLabel(aReader);
		//             mCurrentOffset = aReader.Position;
		//             mNextLabel = GetInstructionLabel(aReader.NextPosition);
		//             mCurrentMethodInfo = aMethodInfo;
		//         }
		// 
		// 	    public override void DoAssemble() {
		//             EmitNotImplementedException(Assembler, GetServiceProvider(), "Sub_Ovf instruction not yet implemented",
		//                 mCurrentLabel, mCurrentMethodInfo, mCurrentOffset, mNextLabel);
		// 		}
		// 	}
		// }
		
	}
}
