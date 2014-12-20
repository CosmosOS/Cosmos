using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler;
using Cosmos.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldftn )]
    public class Ldftn : ILOp
    {
        public Ldftn( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Push { DestinationRef = Cosmos.Assembler.ElementReference.New( LabelName.Get(((OpMethod)aOpCode).Value ) ) };
        }
    }
}