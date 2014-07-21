using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_R8)]
	public class Conv_R8: ILOp
	{
		public Conv_R8(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
        var xSource = aOpCode.StackPopTypes[0];
        var xSourceSize = SizeOfType(xSource);
        switch (xSourceSize)
        {
            case 1:
            case 2:
            case 4:
				if (TypeIsFloat(xSource))
				{
					new CPUx86.SSE.ConvertSS2SD { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
				}
				else
				{
					new CPUx86.SSE.ConvertSI2SD { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
				}
				// expand stack, that moved data is valid stack
				new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
				new CPUx86.SSE.MoveSD { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM0 };
				break;
            case 8:
                {
					if (!TypeIsFloat(xSource))
					{
						new CPUx86.x87.IntLoad { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
						new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true};
					}
                    break;
                }
            default:
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                throw new NotImplementedException();
		}
    }
	}
}