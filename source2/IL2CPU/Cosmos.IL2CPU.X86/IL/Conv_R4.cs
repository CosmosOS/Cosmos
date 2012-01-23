using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

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
            var xSource = Assembler.Stack.Peek();
            
            Assembler.Stack.Pop();
            switch (xSource.Size)
            {
                case 1:
                case 2:
                    {
                        new CPUx86.SSE.ConvertSI2SS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                        new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM0 };
                        break;
                    }
                case 4:
                    {
                        if (!xSource.IsFloat)
                        {
                            if (xSource.IsSigned)
                            {
                                new CPUx86.SSE.ConvertSI2SS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM0 };
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
                        if (xSource.IsFloat)
                        {
                            new CPUx86.SSE.ConvertSD2SS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                            new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                        }
                        else
                        {
                            if (xSource.IsSigned)
                            {
                                new CPUx86.x87.IntLoad { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
                                new CPUx86.SSE.ConvertSD2SS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                                new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 32, DestinationIsIndirect = true };
                                
                                //throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Conversion of Int64 to Float is not yet implemented!");
                            }
                            else
                            {
                                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_R4.cs->Conversion of UInt64 to Float is not yet implemented!");
                            }
                        }
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U4: SourceSize " + xStackItem.Size + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
            //Assembler.Stack.Push(4, typeof(uint));
            Assembler.Stack.Push(4, typeof(float));
        }
    }
}