using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stobj )]
    public class Stobj : ILOp
    {
        public Stobj( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xFieldSize = SizeOfType(aOpCode.StackPopTypes[0]);
            var xRoundedSize = Align(xFieldSize, 4);
            DoNullReferenceCheck(Assembler, DebugEnabled, xRoundedSize);

            new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = checked((int)xRoundedSize) };
            for( int i = 0; i < ( xFieldSize / 4 ); i++ )
            {
                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = i * 4, SourceReg = CPUx86.RegistersEnum.EAX };
            }
            switch( xFieldSize % 4 )
            {
                case 1:
                    {
                        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = checked((int)( xFieldSize / 4 ) * 4 ), SourceReg = CPUx86.RegistersEnum.AL };
                        break;
                    }
                case 2:
                    {
                        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = checked((int)( xFieldSize / 4 ) * 4 ), SourceReg = CPUx86.RegistersEnum.AX };
                        break;
                    }
                case 0:
                    {
                        break;
                    }
                default:
                    throw new Exception( "Remainder size " + ( xFieldSize % 4 ) + " not supported!" );
            }
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
        }
    }
}
