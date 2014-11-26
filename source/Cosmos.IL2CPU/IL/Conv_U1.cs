using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to UInt8(byte) and extends it to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_U1)]
    public class Conv_U1 : ILOp
    {
        public Conv_U1(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceIsFloat = TypeIsFloat(xSource);
            var xSourceSize = SizeOfType(xSource);
            if (xSourceIsFloat)
            {
                if (xSourceSize == 4)
                {
                    new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                    new CPUx86.SSE.ConvertSS2SIAndTruncate { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                }
                else if (xSourceSize == 8)
                {
                    new CPUx86.SSE.MoveDoubleAndDupplicate { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.ConvertSD2SIAndTruncate { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.XMM0, };
                    new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                }
                else
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_U1.cs->Unknown size of floating point value.");
                }
            }
            else { 
                switch (xSourceSize)
                {
                    case 1:
                    case 2:
                    case 4:
                        {
                            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                            new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.AL, Size = 8 };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                            break;
                        }
                    case 8:
                        {
                            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                            new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.AL, Size = 8 };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                            break;
                        }
                    default:
                        //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                        throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U1.cs->Unknown size of variable on the top of the stack.");
                }
            }
        }
    }
}