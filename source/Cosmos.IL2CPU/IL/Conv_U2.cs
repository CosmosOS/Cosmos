using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to UInt16 and extends it to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_U2)]
    public class Conv_U2 : ILOp
    {
        public Conv_U2(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSize = SizeOfType(xSource);
            var xIsFloat = TypeIsFloat(xSource);

            if (xSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U2.cs->Error: StackSize > 8 not supported");
            }

            if (xSize <= 4)
            {
                if (xIsFloat)
                {
                    XS.SSE.ConvertSS2SIAndTruncateIndirectSource(EAX, ESP);
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.MoveZeroExtend(EAX, AX);
                    XS.Push(EAX);
                }
            }
            else if (xSize <= 8)
            {
                if (xIsFloat)
                {
                    XS.SSE2.ConvertSD2SIAndTruncateIndirectSource(EAX, ESP);
                    // We need to move the stack pointer of 4 Byte to "eat" the second double that is yet in the stack or we get a corrupted stack!
                    XS.Add(ESP, 8);
                    XS.Push(EAX);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.Add(ESP, 4);
                    XS.MoveZeroExtend(EAX, AX);
                    XS.Push(EAX);
                }
            }
        }
    }
}
