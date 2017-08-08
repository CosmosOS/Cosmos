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

    //    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
    //    {
    //        var xSource = Assembler.Stack.Peek();
    //        if (xSource.IsFloat)
    //        {
    //            new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
    //            XS.ConvertSS2SI(XSRegisters.EAX, XSRegisters.CPUx86.Registers.XMM0);
    //            XS.Mov(XSRegisters.ESP, XSRegisters.EAX, destinationIsIndirect: true);
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
    //                        XS.Pop(XSRegisters.EAX);
    //                        XS.Pop(XSRegisters.EDX);
    //                        XS.Push(XSRegisters.EAX);
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