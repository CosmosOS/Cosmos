using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
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
                        new CPUx86.SSE.MoveSS
                        {
                            DestinationReg = CPUx86.RegistersEnum.XMM0,
                            SourceReg = CPUx86.RegistersEnum.ESP,
                            SourceIsIndirect = true
                        };
                        new CPUx86.Add
                        {
                            DestinationReg = CPUx86.RegistersEnum.ESP,
                            SourceValue = 4
                        };
                        new CPUx86.SSE.MoveSS
                        {
                            DestinationReg = CPUx86.RegistersEnum.XMM1,
                            SourceReg = CPUx86.RegistersEnum.ESP,
                            SourceIsIndirect = true
                        };
                        new CPUx86.SSE.SubSS
                        {
                            DestinationReg = CPUx86.RegistersEnum.XMM1,
                            SourceReg = CPUx86.RegistersEnum.XMM0
                        };
                        new CPUx86.SSE.MoveSS
                        {
                            DestinationReg = CPUx86.RegistersEnum.ESP,
                            DestinationIsIndirect = true,
                            SourceReg = CPUx86.RegistersEnum.XMM1
                        };
                    }
                    else
                    {
                        new CPUx86.Pop
                        {
                            DestinationReg = CPUx86.RegistersEnum.ECX
                        };
                        new CPUx86.Pop
                        {
                            DestinationReg = CPUx86.RegistersEnum.EAX
                        };
                        XS.Sub(XSRegisters.OldToNewRegister(RegistersEnum.EAX), XSRegisters.OldToNewRegister(RegistersEnum.ECX));
                        new CPUx86.Push
                        {
                            DestinationReg = CPUx86.RegistersEnum.EAX
                        };
                    }
                    break;
                case 8:
                    if (xStackTopIsFloat)
                    {
                        new CPUx86.x87.FloatLoad
                        {
                            DestinationReg = RegistersEnum.ESP,
                            Size = 64,
                            DestinationIsIndirect = true,
                            DestinationDisplacement = 8
                        };
                        new CPUx86.x87.FloatSub
                        {
                            DestinationReg = CPUx86.RegistersEnum.ESP,
                            DestinationIsIndirect = true,
                            Size = 64
                        };
                        new CPUx86.Add
                        {
                            SourceValue = 8,
                            DestinationReg = RegistersEnum.ESP
                        };
                        new CPUx86.x87.FloatStoreAndPop
                        {
                            DestinationReg = RegistersEnum.ESP,
                            Size = 64,
                            DestinationIsIndirect = true
                        };
                    }
                    else
                    {
                        new CPUx86.Pop
                        {
                            DestinationReg = CPUx86.RegistersEnum.EAX
                        };
                        new CPUx86.Pop
                        {
                            DestinationReg = CPUx86.RegistersEnum.EDX
                        };
                        new CPUx86.Sub
                        {
                            DestinationReg = CPUx86.RegistersEnum.ESP,
                            DestinationIsIndirect = true,
                            SourceReg = CPUx86.RegistersEnum.EAX
                        };
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
