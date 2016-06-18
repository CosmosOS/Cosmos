using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

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
                        XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                        XS.Add(OldToNewRegister(RegistersEnum.ESP), 4);
                        XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                        XS.SSE.SubSS(XMM0, XMM1);
                        XS.SSE.MoveSS(ESP, XMM1, destinationIsIndirect: true);
                    }
                    else
                    {
                        XS.Pop(OldToNewRegister(RegistersEnum.ECX));
                        XS.Pop(OldToNewRegister(RegistersEnum.EAX));
                        XS.Sub(OldToNewRegister(RegistersEnum.EAX), OldToNewRegister(RegistersEnum.ECX));
                        XS.Push(OldToNewRegister(RegistersEnum.EAX));
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
                        XS.Add(ESP, 8);
                        XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Long64);
                    }
                    else
                    {
                        XS.Pop(OldToNewRegister(RegistersEnum.EAX));
                        XS.Pop(OldToNewRegister(RegistersEnum.EDX));
                        XS.Sub(ESP, EAX, destinationIsIndirect: true);
                        XS.SubWithCarry(ESP, EDX, destinationDisplacement: 4);
                    }
                    break;
                default:
                    throw new NotImplementedException("not implemented");
            }
        }
    }
}
