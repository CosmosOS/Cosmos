using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_R8)]
	public class Conv_R8: ILOp
	{
		public Conv_R8(Cosmos.Compiler.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
        var xSource = Assembler.Stack.Peek();
        if (!xSource.IsFloat)
        {
            new CPUx86.Move { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
            new CPUx86.SSE.ConvertSI2SS { SourceReg = CPUx86.Registers.EAX, DestinationReg = CPUx86.Registers.XMM0 };
            new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
        }
        Assembler.Stack.Pop();
        switch (xSource.Size)
        {
            case 1:
            case 2:
            case 4:
                {

                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Push { DestinationValue = 0 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                }
            case 8:
                {
                    break;
                }
            default:
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                throw new NotImplementedException();
		}
		Assembler.Stack.Push(8, typeof(double));
    }
	}
}