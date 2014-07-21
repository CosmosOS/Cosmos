using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to Int64.
	/// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I8 )]
    public class Conv_I8 : ILOp
    {
        public Conv_I8( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);
            switch( xSourceSize )
            {
                case 1:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I8.cs->The size 1 could not exist, because always is pushed Int32 or Int64!");
                case 2:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I8.cs->The size 2 could not exist, because always is pushed Int32 or Int64!");
                case 4:
					if (xSourceIsFloat)
					{
						new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32 };
						new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
						new CPUx86.x87.IntStoreWithTrunc { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
					}
					else
					{
						new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
						new CPUx86.SignExtendAX { Size = 32 };
						new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
					}
                    break;
                case 8:
					if (xSourceIsFloat)
					{
						new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
						new CPUx86.x87.IntStoreWithTrunc { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
					}
                    break;
                default:
                    //EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
                    throw new NotImplementedException();
            }
        }
    }
}