using System;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using System.Collections.Generic;


namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldc_I8 )]
    public class Ldc_I8 : ILOp
    {
        public Ldc_I8( Cosmos.Assembler.Assembler aAsmblr ) : base( aAsmblr ) { }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode ) {
            var xOp = (OpInt64)aOpCode;
			// push high part
            new CPUx86.Push { DestinationValue = BitConverter.ToUInt32(BitConverter.GetBytes(xOp.Value), 4) };
			// push low part
			new CPUx86.Push { DestinationValue = BitConverter.ToUInt32(BitConverter.GetBytes(xOp.Value), 0) };
        }
    }
}