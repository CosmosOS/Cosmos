using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldelem_U1)]
	public class Ldelem_U1: ILOp
	{
		public Ldelem_U1(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      //TODO: Implement this Op
    }

    
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldelem_U1)]
		// 	public class Ldelem_U1: Op {
		// 
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
		//         //{
		//         //    Engine.RegisterType(typeof(byte));
		//         //}
		// 
		//         public Ldelem_U1(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		
		// 		public override void DoAssemble() {
		// 			Ldelem_Ref.Assemble(Assembler, 1);
		// 		}
		// 	}
		// }
		
	}
}
