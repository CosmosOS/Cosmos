using System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Converts the value on top of the evaluation stack to float32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_R4)]
    public class Conv_R4 : ILOp
    {
        public Conv_R4(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceIsFloat = TypeIsFloat(xSource);
            var xSourceSize = SizeOfType(xSource);
            switch (xSourceSize)
            {
                case 1:
                case 2:
                    {
                        XS.SSE.ConvertSI2SS(XMM0, ESP, sourceIsIndirect: true);
                        XS.SSE.MoveSS(ESP, XMM0, destinationIsIndirect: true);
                        break;
                    }
                case 4:
                    {
                        if (!xSourceIsFloat)
                        {
                            if (IsIntegerSigned(xSource))
                            {
                                XS.SSE.ConvertSI2SS(XMM0, ESP, sourceIsIndirect: true);
                                XS.SSE.MoveSS(ESP, XMM0, destinationIsIndirect: true);
                            }
                            else
                            {
                                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Conversion of UInt32 to Float is not yet implemented!");
                            }
                         }
                         break;
                     }
                 case 8:
                     {
                         if (xSourceIsFloat)
                         {
                             XS.SSE2.ConvertSD2SS(XMM0, ESP, sourceIsIndirect: true);
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
                                /* This instruction is not needed FloatStoreAndPop does already the conversion */
                                // XS.SSE2.ConvertSD2SS(XMM0, ESP, sourceIsIndirect: true);
                                XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Int32);
                            }
                            else
                            {
                                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Conversion of UInt64 to Float is not yet implemented!");
                            }
                        }

                        // Why I need to do all this Pop / Pop / Pushing or I get stack corruption? The result in the stack as expected so?
                        XS.Pop(XSRegisters.EAX);
                        XS.Pop(XSRegisters.ECX);
                        XS.Push(XSRegisters.EAX);
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U4: SourceSize " + xStackItem.Size + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
        }
    }
}
