using System;

using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Conv_R8)]
    public class Conv_R8 : ILOp
    {
        public Conv_R8(Cosmos.Assembler.Assembler aAsmblr) : base(aAsmblr)
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);

            if (xSourceSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R8.cs->Error: StackSize > 8 not supported");
            }

            if (xSourceSize <= 4)
            {
                if (xSourceIsFloat)
                {
                    XS.SSE.ConvertSS2SD(XMM0, ESP, sourceIsIndirect: true);
                }
                else
                {
                    if (IsIntegerSigned(xSource))
                    {
                        XS.SSE2.ConvertSI2SD(XMM0, ESP, sourceIsIndirect: true);
                    }
                    else
                    {
                        XS.Set(EAX, ESP, sourceIsIndirect: true);

                        // store the last bit of EAX in EBX
                        XS.Set(EBX, 1);
                        XS.And(EBX, ESP, sourceIsIndirect: true);

                        // divide by 2
                        XS.ShiftRight(EAX, 1);

                        // convert
                        XS.SSE2.ConvertSI2SD(XMM0, EAX);

                        // multiply by 2
                        XS.Set(EAX, 2);
                        XS.SSE2.ConvertSI2SD(XMM1, EAX);
                        XS.SSE2.MulSD(XMM0, XMM1);

                        // add the truncated bit
                        XS.SSE2.ConvertSI2SD(XMM1, EBX);
                        XS.SSE2.AddSD(XMM0, XMM1);
                    }
                }

                XS.Sub(ESP, 4);
                XS.SSE2.MoveSD(ESP, XMM0, destinationIsIndirect: true);
            }
            else if (xSourceSize <= 8)
            {
                if (!xSourceIsFloat)
                {
                    if (IsIntegerSigned(xSource))
                    {
                        XS.FPU.IntLoad(ESP, isIndirect: true, size: RegisterSize.Long64);
                        XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Long64);
                    }
                    else
                    {
                        // low
                        XS.Pop(EAX);
                        // high
                        XS.Pop(EDX);

                        // store the last bit of EAX in EBX
                        XS.Set(EBX, 1);
                        XS.And(EBX, EAX);

                        // divide by 2
                        XS.ShiftRightDouble(EAX, EDX, 1);

                        XS.Push(EDX);
                        XS.Push(EAX);

                        // convert
                        XS.FPU.IntLoad(ESP, isIndirect: true, size: RegisterSize.Long64);
                        XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Long64);

                        // multiply by 2
                        XS.Set(EAX, 2);
                        XS.SSE2.ConvertSI2SD(XMM1, EAX);
                        XS.SSE2.MulSD(XMM0, XMM1);

                        // add the truncated bit
                        XS.SSE2.ConvertSI2SD(XMM1, EBX);
                        XS.SSE2.AddSD(XMM0, XMM1);

                        XS.SSE2.MoveSD(ESP, XMM0, destinationIsIndirect: true);
                    }
                }
            }
        }
    }
}
