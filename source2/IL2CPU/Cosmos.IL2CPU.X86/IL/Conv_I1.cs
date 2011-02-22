using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to sbyte and extends it to Int32.
	/// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I1 )]
    public class Conv_I1 : ILOp
    {
        public Conv_I1( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			var xSource = Assembler.Stack.Pop();
            if (xSource.IsFloat)
            {
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                new CPUx86.SSE.ConvertSS2SI { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            }

            switch( xSource.Size )
            {
                case 1:
                case 2:
					throw new Exception("The size {0:D} could not exist, because always is pushed Int32 or Int64!");
                case 4:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.SignExtendAX { Size = 8 };
                    new CPUx86.SignExtendAX { Size = 16 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };

                    break;
                case 8:
                    Assembler.Stack.Pop();
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                    new CPUx86.SignExtendAX { Size = 8 };
                    new CPUx86.SignExtendAX { Size = 16 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException(); 
            }
#if DOTNETCOMPATIBLE
            Assembler.Stack.Push(4, typeof(sbyte));
#else
			Assembler.Stack.Push(1, typeof(sbyte));
#endif
        }
    }
}