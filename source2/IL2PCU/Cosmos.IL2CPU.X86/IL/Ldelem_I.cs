using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldelem_I)]
	public class Ldelem_I: ILOp
	{
		public Ldelem_I(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      throw new Exception("TODO:");
    }

    #region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldelem_I)]
		// 	public class Ldelem_I: Op {
		// 		public Ldelem_I(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		// 			// todo: add support for different pointer sizes
		// 			Ldelem_Ref.Assemble(Assembler, 4);
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
