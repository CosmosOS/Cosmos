using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using CPU = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Shr_Un )]
    public class Shr_Un : ILOp
    {
        public Shr_Un( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            string xBaseLabel = GetLabel( aMethod, aOpCode ) + ".";
            var xStackItem_ShiftAmount = Assembler.Stack.Pop();
            var xStackItem_Value = Assembler.Stack.Pop();
            if( xStackItem_Value.IsFloat ) { throw new NotImplementedException( "Floats not yet supported!" ); }
            if( xStackItem_Value.Size <= 4 )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // shift amount
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX }; // value
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceReg = CPUx86.Registers.AL };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
                Assembler.Stack.Push( xStackItem_Value );
                return;
            }
            if( xStackItem_Value.Size <= 8 )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new Label( xBaseLabel + "__StartLoop" );
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = xBaseLabel + "__EndLoop" };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceValue = 1 };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceValue = 1 };
                new CPUx86.RotateThroughCarryRight { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, Size = 32, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                new CPUx86.Jump { DestinationLabel = xBaseLabel + "__StartLoop" };

                new Label( xBaseLabel + "__EndLoop" );
                Assembler.Stack.Push( xStackItem_Value );
                return;
            }
        }

    }
}
