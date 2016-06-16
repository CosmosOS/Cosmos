using System;
using XSharp.Compiler;
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
                        XS.SSE.ConvertSI2SS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.XMM0 };
                        break;
                    }
                case 4:
                    {
                        if (!xSourceIsFloat)
                        {
                            if (IsIntegerSigned(xSource))
                            {
                                XS.SSE.ConvertSI2SS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.XMM0 };
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
                            new CPUx86.SSE.MoveSS { SourceReg = CPUx86.RegistersEnum.XMM0, DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
                        }
                        else
                        {
                            if (IsIntegerSigned(xSource))
                            {
                                new CPUx86.x87.IntLoad { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
                                new CPUx86.SSE.ConvertSD2SS { SourceReg = CPUx86.RegistersEnum.ESP, DestinationReg = CPUx86.RegistersEnum.XMM0, SourceIsIndirect = true };
                                new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 32, DestinationIsIndirect = true };

                                //throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Conversion of Int64 to Float is not yet implemented!");
                            }
                            else
                            {
                                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Conversion of UInt64 to Float is not yet implemented!");
                            }
                        }
                        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                        XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U4: SourceSize " + xStackItem.Size + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
        }
    }
}
