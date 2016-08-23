using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using static XSharp.Compiler.XSRegisters;

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
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.SSE.ConvertSS2SIAndTruncate(EAX, XMM0);
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
                }
                else if (xSourceSize == 8)
                {
                    XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                    XS.SSE2.ConvertSD2SIAndTruncate(EAX, XMM0);
                    // We need to move the stack pointer of 4 Byte to "eat" the second double that is yet in the stack or we get a corrupted stack!
                    XS.Add(ESP, 4);
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
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
                            XS.Pop(EAX);
                            XS.MoveZeroExtend(EAX, AL);
                            XS.Push(EAX);
                            break;
                        }
                    case 8:
                        {
                            XS.Pop(EAX);
                            XS.Pop(ECX);
                            XS.MoveZeroExtend(EAX, AL);
                            XS.Push(EAX);
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
