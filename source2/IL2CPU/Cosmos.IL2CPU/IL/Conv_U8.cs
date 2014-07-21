using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to UInt64 and change its type to Int64.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_U8)]
    public class Conv_U8 : ILOp
    {
        public Conv_U8(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            switch (xSourceSize)
            {
                case 1:
                case 2:
                case 4:
                    {
                        if (TypeIsFloat(xSource))
                        {
                            new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 32, DestinationIsIndirect = true };
                            new CPUx86.Sub { DestinationReg = Registers.ESP, SourceValue = 4 };
                            new CPUx86.x87.IntStoreWithTrunc { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
                        }
                        else
                        {
                            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                            new CPUx86.Push { DestinationValue = 0 };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        }
                        break;
                    }
                case 8:
                    {
                        if (TypeIsFloat(xSource))
                        {
                            new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
                            new CPUx86.x87.FloatABS();
                            new CPUx86.x87.IntStoreWithTrunc { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
                        }
                        //Else it's already an Int64, or UInt64
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U8.cs->Unknown size of variable on the top of the stack.");
            }
        }
    }
}