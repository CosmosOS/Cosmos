using System;
using CPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Sub )]
    public class Sub : ILOp
    {
        public Sub( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var stackTop = Assembler.Stack.Pop();

            if( stackTop.IsFloat )
            {
                throw new NotImplementedException( "not implemented" );

            }

            switch( stackTop.Size )
            {
                case 1:
                case 2:
                case 4:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                case 8:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                    new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                    new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EDX };
                    break;
                default:
                    throw new NotImplementedException( "not implemented" );
            }
        }

    }
}
