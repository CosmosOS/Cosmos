using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to Int16 and extends it to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_I2)]
    public class Conv_I2 : ILOp
    {
        public Conv_I2(Cosmos.Assembler.Assembler aAsmblr)
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
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_I2.cs->Error: StackSize > 8 not supported");
            }

            if (xSize <= 4)
            {
                if (xIsFloat)
                {
                    XS.SSE.ConvertSS2SIAndTruncateIndirectSource(EAX, ESP);
                    XS.MoveSignExtend(EAX, AX);
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.MoveSignExtend(EAX, AX);
                    XS.Push(EAX);
                }
            }
            else if (xSize <= 8)
            {
                if (xIsFloat)
                {
                    XS.SSE2.ConvertSD2SIAndTruncateIndirectSource(EAX, ESP);
                    XS.Add(ESP, 4);
                    XS.MoveSignExtend(EAX, AX);
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.Pop(EBX);
                    XS.MoveSignExtend(EAX, AX);
                    XS.Push(EAX);
                }
            }
        }
    }
}
