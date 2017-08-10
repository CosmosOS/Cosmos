using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;


namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Sub_Ovf)]
	public class Sub_Ovf: ILOp
	{
		public Sub_Ovf(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr) {
		}

		public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode) {
            //if (Assembler.Stack.Peek().IsFloat) {
            //    throw new NotImplementedException("Sub_Ovf: TODO need to call Sub IL");
            //}
			throw new NotImplementedException();
		}
	}
}