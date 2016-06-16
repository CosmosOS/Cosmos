using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using Label = Cosmos.Assembler.Label;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Mul)]
    public class Mul : ILOp
    {
        public Mul(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xStackContent = aOpCode.StackPopTypes[0];
            var xStackContentSize = SizeOfType(xStackContent);
            var xStackContentIsFloat = TypeIsFloat(xStackContent);
            string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
            DoExecute(xStackContentSize, xStackContentIsFloat, BaseLabel);
        }

        public static void DoExecute(uint xStackContentSize, bool xStackContentIsFloat, string aBaseLabel)
        {
            if (xStackContentSize > 4)
            {
                if (xStackContentIsFloat)
                {
                    new CPUx86.x87.FloatLoad
                    {
                        DestinationReg = CPUx86.RegistersEnum.ESP,
                        Size = 64,
                        DestinationIsIndirect = true
                    };
                    new CPUx86.Add
                    {
                        SourceValue = 8,
                        DestinationReg = CPUx86.RegistersEnum.ESP
                    };
                    new CPUx86.x87.FloatMul
                    {
                        DestinationReg = CPUx86.RegistersEnum.ESP,
                        DestinationIsIndirect = true,
                        Size = 64
                    };
                    new CPUx86.x87.FloatStoreAndPop
                    {
                        DestinationReg = CPUx86.RegistersEnum.ESP,
                        Size = 64,
                        DestinationIsIndirect = true
                    };
                }
                else
                {
                    // div of both == LEFT_LOW * RIGHT_LOW + ((LEFT_LOW * RIGHT_HIGH + RIGHT_LOW * LEFT_HIGH) << 32)
                    string Simple32Multiply = aBaseLabel + "Simple32Multiply";
                    string MoveReturnValue = aBaseLabel + "MoveReturnValue";

                    // right value
                    // low
                    //  SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true
                    // high
                    //  SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4

                    // left value
                    // low
                    //  SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 8
                    // high
                    //  SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 12

                    // compair LEFT_HIGH, RIGHT_HIGH , on zero only simple multiply is used
                    //mov RIGHT_HIGH to eax, is useable on Full 64 multiply
                    XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 4);
                    XS.Or(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 12);
                    XS.Jump(CPUx86.ConditionalTestEnum.Zero, Simple32Multiply);
                    // Full 64 Multiply

                    // copy again, or could change EAX
                    //TODO is there an opcode that does OR without change EAX?
                    XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 4);
                    // eax contains already RIGHT_HIGH
                    // multiply with LEFT_LOW
                    XS.Multiply(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), displacement: 8);
                    // save result of LEFT_LOW * RIGHT_HIGH
                    XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

                    //mov RIGHT_LOW to eax
                    XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
                    // multiply with LEFT_HIGH
                    XS.Multiply(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), displacement: 12);
                    // add result of LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

                    //mov RIGHT_LOW to eax
                    XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
                    // multiply with LEFT_LOW
                    XS.Multiply(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), displacement: 8);
                    // add LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH to high dword of last result
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));

                    new CPUx86.Jump
                    {
                        DestinationLabel = MoveReturnValue
                    };

                    XS.Label(Simple32Multiply);
                    //mov RIGHT_LOW to eax
                    XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
                    // multiply with LEFT_LOW
                    XS.Multiply(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), displacement: 8);

                    XS.Label(MoveReturnValue);
                    // move high result to left high
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.RegistersEnum.ESP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = 12,
                        SourceReg = CPUx86.RegistersEnum.EDX
                    };
                    // move low result to left low
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.RegistersEnum.ESP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = 8,
                        SourceReg = CPUx86.RegistersEnum.EAX
                    };
                    // pop right 64 value
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 8);
                }
            }
            else
            {
                if (xStackContentIsFloat)
                {
                    XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                    XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.SSE.MulSS(XSRegisters.XMM0, XSRegisters.XMM1);
                    XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                }
                else
                {
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    new CPUx86.Multiply
                    {
                        DestinationReg = CPUx86.RegistersEnum.ESP,
                        DestinationIsIndirect = true,
                        Size = 32
                    };
                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                }
            }
        }
    }
}
