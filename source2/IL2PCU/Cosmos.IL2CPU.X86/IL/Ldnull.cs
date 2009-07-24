using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldnull)]
	public class Ldnull: ILOpX86
	{
		public Ldnull(ILOpCode aOpCode):base(aOpCode)
		{
		}

		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldnull)]
		// 	public class Ldnull: Op {
		// 		public Ldnull(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		}
		// 		public override void DoAssemble() {
		//             new CPU.Push { DestinationValue = 0 };
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
