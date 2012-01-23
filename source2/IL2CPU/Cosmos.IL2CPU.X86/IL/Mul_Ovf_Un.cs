using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Mul_Ovf_Un)]
	public class Mul_Ovf_Un: ILOp
	{
		public Mul_Ovf_Un(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
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
		// 	[Cosmos.Assembler.OpCode(OpCodeEnum.Mul_Ovf_Un)]
		// 	public class Mul_Ovf_Un: Mul_Ovf {
		// 		public Mul_Ovf_Un(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 	}
		// }
		
	}
}
