using System;
using Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Beq )]
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Bge )]
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Bgt )]
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ble )]
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Blt )]
    public class Branch : ILOp
    {

        public Branch( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackContent = Assembler.StackContents.Pop();
            Assembler.StackContents.Pop();
            if( xStackContent.Size > 8 )
            {
                throw new Exception( "StackSize > 8 not supported" );
            }

            CPU.ConditionalTestEnum xTestOp;
            switch( aOpCode.OpCode )
            {
                case ILOpCode.Code.Beq:
                    xTestOp = CPU.ConditionalTestEnum.Zero;
                    break;
                case ILOpCode.Code.Bge:
                    xTestOp = CPU.ConditionalTestEnum.GreaterThanOrEqualTo;
                    break;
                case ILOpCode.Code.Bgt:
                    xTestOp = CPU.ConditionalTestEnum.GreaterThan;
                    break;
                case ILOpCode.Code.Ble:
                    xTestOp = CPU.ConditionalTestEnum.LessThanOrEqualTo;
                    break;
                case ILOpCode.Code.Blt:
                    xTestOp = CPU.ConditionalTestEnum.LessThan;
                    break;
                default:
                    throw new Exception( "Unknown OpCode for conditional branch." );
                    break;
            }

            if( xStackContent.Size <= 4 )
            {
                new CPU.Pop { DestinationReg = CPU.Registers.EAX };
                new CPU.Pop { DestinationReg = CPU.Registers.EBX };
                new CPU.Compare { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.EBX };
                new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AssemblerNasm.TmpBranchLabel( aMethod, aOpCode ) };
            }
            else
            {
                new CPU.Pop { DestinationReg = CPU.Registers.EAX };
                new CPU.Pop { DestinationReg = CPU.Registers.EBX };
                new CPU.Pop { DestinationReg = CPU.Registers.ECX };
                new CPU.Pop { DestinationReg = CPU.Registers.EDX };
                new CPU.Xor { DestinationReg = CPU.Registers.EAX, SourceReg = CPU.Registers.ECX };
                new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AssemblerNasm.TmpBranchLabel( aMethod, aOpCode ) };
                new CPU.Xor { DestinationReg = CPU.Registers.EBX, SourceReg = CPU.Registers.EDX };
                new CPU.ConditionalJump { Condition = xTestOp, DestinationLabel = AssemblerNasm.TmpBranchLabel( aMethod, aOpCode ) };
            }
        }

    }
}
