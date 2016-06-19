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
                            new CPUx86.SSE.ConvertSD2SS { SourceReg = CPUx86.RegistersEnum.ESP, DestinationReg = CPUx86.RegistersEnum.XMM0, SourceIsIndirect = true };
                            XS.SSE.MoveSS(ESP, XMM0, destinationIsIndirect: true);
                        }
                        else
                        {
                            if (IsIntegerSigned(xSource))
                            {
                                XS.FPU.IntLoad(ESP, isIndirect: true, size: RegisterSize.Long64);
                                new CPUx86.SSE.ConvertSD2SS { SourceReg = CPUx86.RegistersEnum.ESP, DestinationReg = CPUx86.RegistersEnum.XMM0, SourceIsIndirect = true };
                                XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Int32);

                                //throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Conversion of Int64 to Float is not yet implemented!");
                            }
                            else
                            {
                                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Conversion of UInt64 to Float is not yet implemented!");
                            }
                        }
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
