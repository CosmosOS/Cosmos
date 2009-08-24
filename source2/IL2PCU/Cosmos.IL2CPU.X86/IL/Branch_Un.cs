using System;
using Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Bne_Un )]
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Bge_Un )]
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Bgt_Un )]
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ble_Un )]
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Blt_Un )]
    public class Branch_Un : ILOp
    {

        public Branch_Un( Cosmos.IL2CPU.Assembler aAsmblr )
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
                case ILOpCode.Code.Bne_Un:
                    xTestOp = CPU.ConditionalTestEnum.NotEqual;
                    break;
                case ILOpCode.Code.Bge_Un:
                    xTestOp = CPU.ConditionalTestEnum.AboveOrEqual;
                    break;
                case ILOpCode.Code.Bgt_Un:
                    xTestOp = CPU.ConditionalTestEnum.Above;
                    break;
                case ILOpCode.Code.Ble_Un:
                    xTestOp = CPU.ConditionalTestEnum.BelowOrEqual;
                    break;
                case ILOpCode.Code.Blt_Un:
                    xTestOp = CPU.ConditionalTestEnum.Below;
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
