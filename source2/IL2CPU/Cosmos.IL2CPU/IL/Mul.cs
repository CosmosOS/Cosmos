using System;
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
                        DestinationReg = CPUx86.Registers.ESP,
                        Size = 64,
                        DestinationIsIndirect = true
                    };
                    new CPUx86.Add
                    {
                        SourceValue = 8,
                        DestinationReg = CPUx86.Registers.ESP
                    };
                    new CPUx86.x87.FloatMul
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        Size = 64
                    };
                    new CPUx86.x87.FloatStoreAndPop
                    {
                        DestinationReg = CPUx86.Registers.ESP,
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
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.Registers.EAX,
                        SourceReg = CPUx86.Registers.ESP,
                        SourceIsIndirect = true,
                        SourceDisplacement = 4
                    };
                    new CPUx86.Or
                    {
                        DestinationReg = CPUx86.Registers.EAX,
                        SourceReg = CPUx86.Registers.ESP,
                        SourceIsIndirect = true,
                        SourceDisplacement = 12
                    };
                    new CPUx86.ConditionalJump
                    {
                        Condition = CPUx86.ConditionalTestEnum.Zero,
                        DestinationLabel = Simple32Multiply
                    };
                    // Full 64 Multiply

                    // copy again, or could change EAX
                    //TODO is there an opcode that does OR without change EAX?
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.Registers.EAX,
                        SourceReg = CPUx86.Registers.ESP,
                        SourceIsIndirect = true,
                        SourceDisplacement = 4
                    };
                    // eax contains already RIGHT_HIGH
                    // multiply with LEFT_LOW
                    new CPUx86.Multiply
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = 8,
                        Size = 32
                    };
                    // save result of LEFT_LOW * RIGHT_HIGH
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.Registers.ECX,
                        SourceReg = CPUx86.Registers.EAX
                    };

                    //mov RIGHT_LOW to eax
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.Registers.EAX,
                        SourceReg = CPUx86.Registers.ESP,
                        SourceIsIndirect = true
                    };
                    // multiply with LEFT_HIGH
                    new CPUx86.Multiply
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = 12,
                        Size = 32
                    };
                    // add result of LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH
                    new CPUx86.Add
                    {
                        DestinationReg = CPUx86.Registers.ECX,
                        SourceReg = CPUx86.Registers.EAX
                    };

                    //mov RIGHT_LOW to eax
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.Registers.EAX,
                        SourceReg = CPUx86.Registers.ESP,
                        SourceIsIndirect = true
                    };
                    // multiply with LEFT_LOW
                    new CPUx86.Multiply
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = 8,
                        Size = 32
                    };
                    // add LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH to high dword of last result
                    new CPUx86.Add
                    {
                        DestinationReg = CPUx86.Registers.EDX,
                        SourceReg = CPUx86.Registers.ECX
                    };

                    new CPUx86.Jump
                    {
                        DestinationLabel = MoveReturnValue
                    };

                    new Label(Simple32Multiply);
                    //mov RIGHT_LOW to eax
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.Registers.EAX,
                        SourceReg = CPUx86.Registers.ESP,
                        SourceIsIndirect = true
                    };
                    // multiply with LEFT_LOW
                    new CPUx86.Multiply
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = 8,
                        Size = 32
                    };

                    new Label(MoveReturnValue);
                    // move high result to left high
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = 12,
                        SourceReg = CPUx86.Registers.EDX
                    };
                    // move low result to left low
                    new CPUx86.Mov
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        DestinationDisplacement = 8,
                        SourceReg = CPUx86.Registers.EAX
                    };
                    // pop right 64 value
                    new CPUx86.Add
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        SourceValue = 8
                    };
                }
            }
            else
            {
                if (xStackContentIsFloat)
                {
                    new CPUx86.SSE.MoveSS
                    {
                        DestinationReg = CPUx86.Registers.XMM0,
                        SourceReg = CPUx86.Registers.ESP,
                        SourceIsIndirect = true
                    };
                    new CPUx86.Add
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        SourceValue = 4
                    };
                    new CPUx86.SSE.MoveSS
                    {
                        DestinationReg = CPUx86.Registers.XMM1,
                        SourceReg = CPUx86.Registers.ESP,
                        SourceIsIndirect = true
                    };
                    new CPUx86.SSE.MulSS
                    {
                        DestinationReg = CPUx86.Registers.XMM1,
                        SourceReg = CPUx86.Registers.XMM0
                    };
                    new CPUx86.SSE.MoveSS
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        SourceReg = CPUx86.Registers.XMM1
                    };
                }
                else
                {
                    new CPUx86.Pop
                    {
                        DestinationReg = CPUx86.Registers.EAX
                    };
                    new CPUx86.Multiply
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        DestinationIsIndirect = true,
                        Size = 32
                    };
                    new CPUx86.Add
                    {
                        DestinationReg = CPUx86.Registers.ESP,
                        SourceValue = 4
                    };
                    new CPUx86.Push
                    {
                        DestinationReg = CPUx86.Registers.EAX
                    };
                }
            }
        }
    }
}