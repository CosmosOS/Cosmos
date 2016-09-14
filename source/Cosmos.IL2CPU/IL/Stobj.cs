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
            DoNullReferenceCheck(Assembler, DebugEnabled, (int)xRoundedSize);

            XS.Set(XSRegisters.ECX, XSRegisters.ESP, sourceDisplacement: checked((int)xRoundedSize));
            for( int i = 0; i < ( xFieldSize / 4 ); i++ )
            {
                XS.Pop(XSRegisters.EAX);
                XS.Set(XSRegisters.ECX, XSRegisters.EAX, destinationDisplacement: i * 4);
            }
            switch( xFieldSize % 4 )
            {
                case 1:
                    {
                        XS.Pop(XSRegisters.EAX);
                        new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ECX, DestinationIsIndirect = true, DestinationDisplacement = checked((int)( xFieldSize / 4 ) * 4 ), SourceReg = CPUx86.RegistersEnum.AL };
                        break;
                    }
                case 2:
                    {
                        XS.Pop(XSRegisters.EAX);
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
            XS.Add(XSRegisters.ESP, 4);
        }
    }
}
