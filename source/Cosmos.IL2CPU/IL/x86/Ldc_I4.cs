using System;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using System.Collections.Generic;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.IL.x86
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldc_I4 )]
    public class Ldc_I4 : ILOp
    {
        public Ldc_I4( Cosmos.Assembler.Assembler aAsmblr ) : base( aAsmblr ) { }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
          XS.Push((uint)((OpInt)aOpCode).Value);
        }

    }
}

