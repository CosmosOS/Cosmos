using System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldelem_I )]
    public class Ldelem_I : ILOp
    {
        public Ldelem_I( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Ldelem_Ref.Assemble( Assembler, 4 );
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
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

    }
}
