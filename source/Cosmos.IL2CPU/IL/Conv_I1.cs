using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using static XSharp.Compiler.XSRegisters;
using static Cosmos.Assembler.x86.SSE.ComparePseudoOpcodes;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to sbyte and extends it to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_I1)]
    public class Conv_I1 : ILOp
    {
        public Conv_I1(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);
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
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
                }
                else
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I1.cs->Unknown size of floating point value.");
                }
            } 

            switch (xSourceSize)
            {
                case 1:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I1.cs->The size 1 could not exist, because always is pushed Int32 or Int64!");
                case 2:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I1.cs->The size 2 could not exist, because always is pushed Int32 or Int64!");
                case 4:
                    XS.Pop(XSRegisters.EAX);
                    XS.MoveSignExtend(XSRegisters.EAX, XSRegisters.AL);
                    XS.Push(XSRegisters.EAX);
                    break;
                case 8:
                    XS.Pop(XSRegisters.EAX);
                    XS.Pop(XSRegisters.EBX);
                    XS.MoveSignExtend(XSRegisters.EAX, XSRegisters.AL);
                    XS.Push(XSRegisters.EAX);
                    break;
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_I1.cs->Unknown size of variable on the top of the stack.");
            }
        }
    }
}
