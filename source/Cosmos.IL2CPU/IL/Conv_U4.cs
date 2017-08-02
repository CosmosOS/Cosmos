using System;

using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to UInt32 and change its type to Int32.
    /// </summary>
    [OpCode(ILOpCode.Code.Conv_U)] // x86 is a 32-bit system, so this is the op-code that we should be using, for an x64 target, use Conv_U8 instead.
    [OpCode(ILOpCode.Code.Conv_U4)]
    public class Conv_U4 : ILOp
    {
        public Conv_U4(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);

            if (xSourceSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U4.cs->Error: StackSize > 8 not supported");
            }

            if (xSourceSize <= 4)
            {
                if (xSourceIsFloat)
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.SSE.ConvertSS2SIAndTruncate(EAX, XMM0);
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
                }
            }
            else if (xSourceSize <= 8)
            {
                if (xSourceIsFloat)
                {
                    XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 4);
                    XS.SSE2.ConvertSD2SIAndTruncate(EAX, XMM0);
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.Add(ESP, 4);
                    XS.Push(EAX);
                }
            }
        }
    }
}
