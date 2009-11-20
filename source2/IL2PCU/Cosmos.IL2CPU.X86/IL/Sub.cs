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

            switch( stackTop.Size )
            {
                case 1:
                case 2:
                case 4:
                    if (stackTop.IsFloat)
                    {
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.SSE.SubSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0 };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM1 };
                    }
                    else
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Sub { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    }
                    break;
                case 8:
                    if (stackTop.IsFloat)
                    {
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM2, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.SSE.SubSS { DestinationReg = CPUx86.Registers.XMM2, SourceReg = CPUx86.Registers.XMM0};
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.XMM2, DestinationIsIndirect = true };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM2, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.SSE.SubSS { DestinationReg = CPUx86.Registers.XMM2, SourceReg = CPUx86.Registers.XMM1 };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.XMM2, DestinationIsIndirect = true };
                        new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    }
                    else
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                        new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                        new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EDX };
                    }
                    break;
                default:
                    throw new NotImplementedException( "not implemented" );
            }
        }

    }
}
