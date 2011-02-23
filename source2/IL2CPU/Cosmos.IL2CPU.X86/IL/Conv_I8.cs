using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to Int64.
	/// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I8 )]
    public class Conv_I8 : ILOp
    {
        public Conv_I8( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = Assembler.Stack.Pop();
            switch( xSource.Size )
            {
                case 1:
                case 2:
                case 4:
					if (xSource.IsFloat)
					{
						new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32 };
						new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
						new CPUx86.x87.IntStoreWithTrunc { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
					}
					else
					{
						new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
						new CPUx86.SignExtendAX { Size = 32 };
						new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
					}
                    break;
                case 8:
					if (xSource.IsFloat)
					{
						new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
						new CPUx86.x87.IntStoreWithTrunc { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
					}
                    break;
                default:
                    //EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
                    throw new NotImplementedException();
            }
            Assembler.Stack.Push(8, typeof(Int64));
        }
    }
}