using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Sub)]
    public class Sub: ILOp
    {
        public Sub(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xStackTop = aOpCode.StackPopTypes[0];
            var xStackTop2 = aOpCode.StackPopTypes[0];
            var xStackTopSize = SizeOfType(xStackTop);
            var xStackTop2Size = SizeOfType(xStackTop2);
            if (xStackTopSize != xStackTop2Size)
            {
                throw new Exception("Different size for substract: " + aMethod.MethodBase + "!");
            }

            var xStackTopIsFloat = TypeIsFloat(xStackTop);

            switch (xStackTopSize)
            {
                case 1:
                case 2:
                case 4:
                    if (xStackTopIsFloat)
                    {
                        XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                        XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                        XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                        XS.SSE.SubSS(XSRegisters.XMM0, XSRegisters.XMM1);
                        XS.SSE.MoveSS(XSRegisters.ESP, XSRegisters.XMM1, destinationIsIndirect: true);
                    }
                    else
                    {
                        XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.ECX));
                        XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
                        XS.Sub(XSRegisters.OldToNewRegister(RegistersEnum.EAX), XSRegisters.OldToNewRegister(RegistersEnum.ECX));
                        XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
                    }
                    break;
                case 8:
                    if (xStackTopIsFloat)
                    {
                        new FloatLoad
                        {
                            DestinationReg = RegistersEnum.ESP,
                            Size = 64,
                            DestinationIsIndirect = true,
                            DestinationDisplacement = 8
                        };
                        new FloatSub
                        {
                            DestinationReg = RegistersEnum.ESP,
                            DestinationIsIndirect = true,
                            Size = 64
                        };
                        XS.Add(XSRegisters.ESP, 8);
                        new FloatStoreAndPop
                        {
                            DestinationReg = RegistersEnum.ESP,
                            Size = 64,
                            DestinationIsIndirect = true
                        };
                    }
                    else
                    {
                        XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
                        XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EDX));
                        XS.Sub(XSRegisters.ESP, XSRegisters.EAX, destinationIsIndirect: true);
                        new CPUx86.SubWithCarry
                        {
                            DestinationReg = CPUx86.RegistersEnum.ESP,
                            DestinationIsIndirect = true,
                            DestinationDisplacement = 4,
                            SourceReg = CPUx86.RegistersEnum.EDX
                        };
                    }
                    break;
                default:
                    throw new NotImplementedException("not implemented");
            }
        }
    }
}
