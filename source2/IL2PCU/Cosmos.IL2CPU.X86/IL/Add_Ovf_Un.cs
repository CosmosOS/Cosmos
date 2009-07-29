using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Add_Ovf_Un)]
	public class Add_Ovf_Un: ILOp
	{
		public Add_Ovf_Un(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

		public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
		{
			throw new NotImplementedException("Add_Ovf_Un not implemented");
		}

	}
}
