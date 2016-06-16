using System;
using XSharp.Compiler;
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
					XS.SSE.ConvertSS2SD(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
				}
				else
				{
					XS.SSE.ConvertSI2SD(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
				}
				// expand stack, that moved data is valid stack
				XS.Sub(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
				XS.SSE2.MoveSD(XSRegisters.ESP, XSRegisters.XMM0, destinationIsIndirect: true);
				break;
            case 8:
                {
					if (!TypeIsFloat(xSource))
					{
						new CPUx86.x87.IntLoad { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
						new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true};
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
