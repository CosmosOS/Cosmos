using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
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
                    XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 8);
                    XS.SSE2.MoveSD(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE2.MulSD(XMM1, XMM0);
                    XS.SSE2.MoveSD(ESP, XMM1, destinationIsIndirect: true);
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
                    XS.Set(EAX, ESP, sourceDisplacement: 4);
                    XS.Or(EAX, ESP, sourceDisplacement: 12);
                    XS.Jump(CPUx86.ConditionalTestEnum.Zero, Simple32Multiply);
                    // Full 64 Multiply

                    // copy again, or could change EAX
                    //TODO is there an opcode that does OR without change EAX?
                    XS.Set(EAX, ESP, sourceDisplacement: 4);
                    // eax contains already RIGHT_HIGH
                    // multiply with LEFT_LOW
                    XS.Multiply(ESP, displacement: 8);
                    // save result of LEFT_LOW * RIGHT_HIGH
                    XS.Set(ECX, EAX);

                    //mov RIGHT_LOW to eax
                    XS.Set(EAX, ESP, sourceIsIndirect: true);
                    // multiply with LEFT_HIGH
                    XS.Multiply(ESP, displacement: 12);
                    // add result of LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH
                    XS.Add(ECX, EAX);

                    //mov RIGHT_LOW to eax
                    XS.Set(EAX, ESP, sourceIsIndirect: true);
                    // multiply with LEFT_LOW
                    XS.Multiply(ESP, displacement: 8);
                    // add LEFT_LOW * RIGHT_HIGH + RIGHT_LOW + LEFT_HIGH to high dword of last result
                    XS.Add(EDX, ECX);

                    XS.Jump(MoveReturnValue);

                    XS.Label(Simple32Multiply);
                    //mov RIGHT_LOW to eax
                    XS.Set(EAX, ESP, sourceIsIndirect: true);
                    // multiply with LEFT_LOW
                    XS.Multiply(ESP, displacement: 8);

                    XS.Label(MoveReturnValue);
                    // move high result to left high
                    XS.Set(ESP, EDX, destinationDisplacement: 12);
                    // move low result to left low
                    XS.Set(ESP, EAX, destinationDisplacement: 8);
                    // pop right 64 value
                    XS.Add(ESP, 8);
                }
            }
            else
            {
                if (xStackContentIsFloat)
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 4);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE.MulSS(XMM1, XMM0);
                    XS.SSE.MoveSS(ESP, XMM1, destinationIsIndirect: true);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.Multiply(ESP, isIndirect: true, size: RegisterSize.Int32);
                    XS.Add(ESP, 4);
                    XS.Push(EAX);
                }
            }
        }
    }
}
