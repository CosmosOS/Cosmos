using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldobj )]
    public class Ldobj : ILOp
    {
        public Ldobj( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            OpType xType = ( OpType )aOpCode;
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            var xObjSize = GetStorageSize(xType.Value);
            for (int i = 1; i <= (xObjSize / 4); i++)
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)(xObjSize - (i * 4)) };
            }

            switch (xObjSize % 4)
            {
                case 1:
                    {
                        new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.BL, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
                        break;
                    }
                case 2:
                    {
                        new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.BX, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
                        break;
                    }
                case 0:
                    {
                        break;
                    }
                default:
                        throw new Exception( "Remainder not supported!" );
            }
        }
    }
}