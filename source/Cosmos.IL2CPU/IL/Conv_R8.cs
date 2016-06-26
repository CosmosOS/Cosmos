using System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
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
					XS.SSE.ConvertSS2SD(XMM0, ESP, sourceIsIndirect: true);
				}
				else
				{
					XS.SSE2.ConvertSI2SD(XMM0, ESP, sourceIsIndirect: true);
				}
				// expand stack, that moved data is valid stack
				XS.Sub(XSRegisters.ESP, 4);
				XS.SSE2.MoveSD(ESP, XMM0, destinationIsIndirect: true);
				break;
            case 8:
                {
					if (!TypeIsFloat(xSource))
					{
						XS.FPU.IntLoad(ESP, isIndirect: true, size: RegisterSize.Long64);
						XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Long64);
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
