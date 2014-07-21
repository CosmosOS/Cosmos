using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Shr_Un )]
    public class Shr_Un : ILOp
    {
        public Shr_Un( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            string xBaseLabel = GetLabel( aMethod, aOpCode ) + ".";
            var xStackItem_ShiftAmount = aOpCode.StackPopTypes[0];
            var xStackItem_Value = aOpCode.StackPopTypes[1];
            if (TypeIsFloat(xStackItem_Value))
            {
                throw new NotImplementedException("Floats not yet supported!");
            }
            var xStackItem_Value_Size = SizeOfType(xStackItem_Value);
            if( xStackItem_Value_Size <= 4 )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // shift amount
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX }; // value
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.CL, SourceReg = CPUx86.Registers.AL };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
                return;
            }
            if( xStackItem_Value_Size <= 8 )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new Label( xBaseLabel + "__StartLoop" );
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = xBaseLabel + "__EndLoop" };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.CL, SourceValue = 1 };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.CL, SourceValue = 1 };
                new CPUx86.RotateThroughCarryRight { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, Size = 32, SourceReg = CPUx86.Registers.CL };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                new CPUx86.Jump { DestinationLabel = xBaseLabel + "__StartLoop" };

                new Label( xBaseLabel + "__EndLoop" );
                return;
            }
        }

    }
}
