using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to UInt64 and change its type to Int64.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_U8)]
    public class Conv_U8 : ILOp
    {
        public Conv_U8(Cosmos.Assembler.Assembler aAsmblr)
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
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U4.cs->Error: StackSize > 8 not supported");
            }

            if (xSize <= 4)
            {
                if (xIsFloat)
                {
                    XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Int32);
                    XS.Sub(ESP, 4);
                    XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.Push(0);
                    XS.Push(EAX);
                }
            }
            else if (xSize <= 8)
            {
                if (xIsFloat)
                {
                    XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Long64);
                    XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
                }
            }
        }
    }
}
