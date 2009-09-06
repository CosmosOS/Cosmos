using System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Add_Ovf )]
    public class Add_Ovf : ILOp
    {
        public Add_Ovf( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            //throw new NotImplementedException( "Add_Ovf not implemented" );
        }

    }
}
