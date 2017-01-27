using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_R8)]
    public class Conv_R8 : ILOp
    {
        public Conv_R8(Cosmos.Assembler.Assembler aAsmblr) : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSize = SizeOfType(xSource);
            var xIsFloat = TypeIsFloat(xSource);
            var xIsSigned = IsIntegerSigned(xSource);

            if (xSize <= 4)
            {
                if (xIsFloat)
                {
                    XS.SSE.ConvertSS2SD(XMM0, ESP, sourceIsIndirect: true);
                }
                else
                {
                    if (xIsSigned)
                    {
                        XS.SSE2.ConvertSI2SD(XMM0, ESP, sourceIsIndirect: true);
                    }
                    else
                    {
                        throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R8.cs->Conversion of UInt32 to Double is not yet implemented!");
                    }
                }

                // expand stack, that moved data is valid stack
                XS.Sub(ESP, 4);
                XS.SSE2.MoveSD(ESP, XMM0, destinationIsIndirect: true);
            }
            else if (xSize <= 8)
            {
                if (!xIsFloat)
                {
                    if (xIsSigned)
                    {
                        XS.FPU.IntLoad(ESP, isIndirect: true, size: RegisterSize.Long64);
                        XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Long64);
                    }
                    else
                    {
                        throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R8.cs->Conversion of UInt64 to Double is not yet implemented!");
                    }
                }
            }
        }
    }
}
