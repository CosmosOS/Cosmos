using System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldelem_R4 )]
    public class Ldelem_R4 : ILOp
    {
        public Ldelem_R4( Cosmos.IL2CPU.Assembler aAsmblr )
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
        // using CPU = Indy.IL2CPU.Assembler.X86;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldelem_R4)]
        // 	public class Ldelem_R4: Op {
        // 		public Ldelem_R4(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        // 			Ldelem_Ref.Assemble(Assembler, 4);
        // 		}
        // 	}
        // }

    }
}
