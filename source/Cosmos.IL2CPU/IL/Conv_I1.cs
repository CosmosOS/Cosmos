using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to sbyte and extends it to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_I1)]
    public class Conv_I1 : ILOp
    {
        public Conv_I1(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);
            if (xSourceIsFloat)
            {
                if (xSourceSize == 4)
                {
                    new CPUx86.SSE.MoveSS { SourceReg = CPUx86.RegistersEnum.ESP, DestinationReg = CPUx86.RegistersEnum.XMM0, SourceIsIndirect = true };
                    XS.SSE.ConvertSS2SIAndTruncate(XSRegisters.EAX, XSRegisters.XMM0);
                    new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, SourceReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
                }
                else if (xSourceSize == 8)
                {
                    XS.SSE3.MoveDoubleAndDuplicate(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.SSE2.ConvertSD2SIAndTruncate(XSRegisters.EAX, XSRegisters.XMM0);
                    new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, SourceReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
                }
                else
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I1.cs->Unknown size of floating point value.");
                }
            }

            switch (xSourceSize)
            {
                case 1:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I1.cs->The size 1 could not exist, because always is pushed Int32 or Int64!");
                case 2:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I1.cs->The size 2 could not exist, because always is pushed Int32 or Int64!");
                case 4:
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    new CPUx86.MoveSignExtend { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.AL, Size = 8 };
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    break;
                case 8:
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX));
                    new CPUx86.MoveSignExtend { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.AL, Size = 8 };
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                    break;
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_I1.cs->Unknown size of variable on the top of the stack.");
            }
        }
    }
}
