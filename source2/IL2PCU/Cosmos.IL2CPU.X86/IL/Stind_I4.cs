using System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stind_I4 )]
    public class Stind_I4 : ILOp
    {
        public Stind_I4( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Stind_I.Assemble( Assembler, 4 );
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPU = Indy.IL2CPU.Assembler.X86;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Stind_I4)]
        // 	public class Stind_I4: Op {
        // 		public Stind_I4(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        // 			Stind_I.Assemble(Assembler, 4);
        // 		}
        // 	}
        // }

    }
}
