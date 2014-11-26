using System;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    //[Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_U )]
    //public class Conv_U : ILOp
    //{
    //    public Conv_U( Cosmos.Assembler.Assembler aAsmblr )
    //        : base( aAsmblr )
    //    {
    //    }

    //    public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
    //    {
    //        var xSource = Assembler.Stack.Peek();
    //        if (xSource.IsFloat)
    //        {
    //            new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
    //            new CPUx86.SSE.ConvertSS2SI { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
    //            new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
    //        }
    //        else
    //        {
    //            Assembler.Stack.Pop();
    //            switch (xSource.Size)
    //            {
    //                case 1:
    //                case 2:
    //                    throw new Exception("The size {0:D} could not exist, because always is pushed Int32 or Int64!");
    //                case 8:
    //                    {
    //                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
    //                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
    //                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
    //                        break;
    //                    }
    //                case 4:
    //                    {
    //                        break;
    //                    }
    //                default:
    //                    //EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_U: SourceSize " + xStackContent.Size + "not supported yet!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
    //                    throw new NotImplementedException();
    //            }
    //            Assembler.Stack.Push(4, typeof(UInt32));
    //        }
    //    }
    //}
}