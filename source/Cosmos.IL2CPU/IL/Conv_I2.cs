using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to Int16 and extends it to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_I2)]
    public class Conv_I2 : ILOp
    {
        public Conv_I2(Cosmos.Assembler.Assembler aAsmblr)
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
                    new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                    new CPUx86.SSE.ConvertSS2SIAndTruncate { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                }
                else if (xSourceSize == 8)
                {
                    new CPUx86.SSE.MoveDoubleAndDupplicate { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.ConvertSD2SIAndTruncate { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.XMM0, };
                    new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                }
                else
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I2.cs->Unknown size of floating point value.");
                }
            }

            switch (xSourceSize)
            {
                  
                case 2:
                    //throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I2.cs->The size 2 could not exist, because always is pushed Int32 or Int64!");
                  break;
                case 1:
                //throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_I2.cs->The size 1 could not exist, because always is pushed Int32 or Int64!");
                case 4:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.MoveSignExtend { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.AX, Size = 16 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                case 8:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                    new CPUx86.MoveSignExtend { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.AX, Size = 16 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                default:
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_I2.cs->SourceSize " + xSource + " not supported!");
            }
        }
    }
}