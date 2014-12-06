using System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ckfinite)]
    public class Ckfinite : ILOp
    {
        public Ckfinite(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Ckfinate.cs->Error: The Ckfinate op-code has not yet been implemented!");
        }

        // using System;
        // using System.IO;
        //
        //
        // using CPU = Cosmos.Assembler.x86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ckfinite)]
        // 	public class Ckfinite: Op {
        // 		public Ckfinite(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        // 			throw new NotImplementedException("This file has been autogenerated and not been changed afterwards!");
        // 		}
        // 	}
        // }
    }
}