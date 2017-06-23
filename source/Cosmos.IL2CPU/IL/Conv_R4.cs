using System;

using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Converts the value on top of the evaluation stack to float32.
    /// </summary>
    [OpCode(ILOpCode.Code.Conv_R4)]
    public class Conv_R4 : ILOp
    {
        public Conv_R4(Cosmos.Assembler.Assembler aAsmblr)
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
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Error: StackSize > 8 not supported");
            }

            if (xSourceSize <= 4)
            {
                if (!xSourceIsFloat)
                {
                    if (IsIntegerSigned(xSource))
                    {
                        XS.SSE.ConvertSI2SS(XMM0, ESP, sourceIsIndirect: true);
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
                        XS.SSE.ConvertSI2SS(XMM0, EAX);

                        // multiply by 2
                        XS.Set(EAX, 2);
                        XS.SSE.ConvertSI2SS(XMM1, EAX);
                        XS.SSE.MulSS(XMM0, XMM1);

                        // add the truncated bit
                        XS.SSE.ConvertSI2SS(XMM1, EBX);
                        XS.SSE.AddSS(XMM0, XMM1);
                    }

                    XS.SSE.MoveSS(ESP, XMM0, destinationIsIndirect: true);
                }
            }
            else if (xSourceSize <= 8)
            {
                if (xSourceIsFloat)
                {
                    XS.SSE2.ConvertSD2SS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 4);
                    XS.SSE.MoveSS(ESP, XMM0, destinationIsIndirect: true);
                }
                else
                {
                    if (IsIntegerSigned(xSource))
                    {
                        /*
                         * Again there is no SSE instruction in x86 to do this conversion as we need a 64 Bit register to do this! So we are forced
                         * to use the legacy x87 FPU to do this operation. In x64 the SSE instruction ConvertSIQ2SS should exist.
                         */
                        XS.FPU.IntLoad(ESP, isIndirect: true, size: RegisterSize.Long64);
                        XS.Add(ESP, 4);
                        /* This instruction is not needed FloatStoreAndPop does already the conversion */
                        // XS.SSE2.ConvertSD2SS(XMM0, ESP, sourceIsIndirect: true);
                        XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Int32);
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
                        XS.Add(ESP, 4);
                        XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Int32);

                        // multiply by 2
                        XS.Set(EAX, 2);
                        XS.SSE.ConvertSI2SS(XMM1, EAX);
                        XS.SSE.MulSS(XMM0, XMM1);

                        // add the truncated bit
                        XS.SSE.ConvertSI2SS(XMM1, EBX);
                        XS.SSE.AddSS(XMM0, XMM1);

                        XS.SSE.MoveSS(ESP, XMM0, destinationIsIndirect: true);
                    }
                }
            }
        }
    }
}
