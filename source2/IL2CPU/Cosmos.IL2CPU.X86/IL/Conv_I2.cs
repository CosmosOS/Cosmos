using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to Int16 and extends it to Int32.
	/// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I2 )]
    public class Conv_I2 : ILOp
    {
        public Conv_I2( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = Assembler.Stack.Pop();
            if (xSource.IsFloat)
            {
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                new CPUx86.SSE.ConvertSS2SI { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            }

            switch( xSource.Size )
            {
                case 1:
                case 2:
                    break;
                case 4:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                case 8:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                    new CPUx86.SignExtendAX { Size = 16 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                default:
                    throw new NotImplementedException( "SourceSize " + xSource + " not supported!" );
            }
#if DOTNETCOMPATIBLE
			Assembler.Stack.Push(4, typeof(Int16));
#else
			Assembler.Stack.Push(2, typeof(Int16));
#endif
        }
    }
}