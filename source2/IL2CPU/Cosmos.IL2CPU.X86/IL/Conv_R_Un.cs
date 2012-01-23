using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Converts the unsigned integer value on top of the evaluation stack to float32 or float64.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_R_Un )]
    public class Conv_R_Un : ILOp
    {
        public Conv_R_Un( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xValue = Assembler.Stack.Peek();
            if (xValue.Size > 8)
            {
                //EmitNotImplementedException( Assembler, aServiceProvider, "Size '" + xSize.Size + "' not supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
                throw new NotImplementedException();
            }
            if (!xValue.IsFloat)
            {
                new CPUx86.Mov { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
                new CPUx86.SSE.ConvertSI2SS { SourceReg = CPUx86.Registers.EAX, DestinationReg = CPUx86.Registers.XMM0 };
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
            }
            Assembler.Stack.Pop();
            switch (xValue.Size)
            {
                case 1:
                case 2:
                case 4:
                    break;
                case 8:
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    break;
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
			Assembler.Stack.Push(4, typeof(float));
        }
    }
}