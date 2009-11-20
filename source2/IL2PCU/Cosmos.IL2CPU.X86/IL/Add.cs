using System;
using CPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Add )]
    public class Add : ILOp
    {
        public Add( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSize = Assembler.Stack.Pop();
            if (xSize.Size > 8)
            {
                //EmitNotImplementedException( Assembler, aServiceProvider, "Size '" + xSize.Size + "' not supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
                throw new NotImplementedException("StackSize>8 not supported");
            }
            else
            {
                if (xSize.Size > 4)
                {
                   new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                   new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                   if (xSize.IsFloat)
                   {
                       //sum the fraction

                       new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                       new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };

                   }
                   else
                   {
                       new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                       new CPUx86.AddWithCarry { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EDX };
                   }
                }
                else
                {
                    if (xSize.IsFloat) //float
                    {
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.SSE.AddSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0 };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM1 };
                    }
                    else //integer
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                    }
                }
            }
        }
    }
}
