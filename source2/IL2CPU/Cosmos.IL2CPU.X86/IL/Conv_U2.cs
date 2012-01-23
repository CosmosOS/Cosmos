using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to UInt16 and extends it to Int32.
	/// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_U2 )]
    public class Conv_U2 : ILOp
    {
        public Conv_U2( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			var xSource = Assembler.Stack.Pop();
            if (xSource.IsFloat)
            {
                if (xSource.Size == 4)
                {
                    new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                    new CPUx86.SSE.ConvertSS2SIAndTruncate { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                }
                else if (xSource.Size == 8)
                {
                    new CPUx86.SSE.MoveDoubleAndDupplicate { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.ConvertSD2SIAndTruncate { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.XMM0, };
                    new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                }
                else
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_U2.cs->Unknown size of floating point value.");
                }
            }

            switch( xSource.Size )
            {
                case 1:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_U2.cs->The size 1 should not exist, because it is always pushed as Int32 or Int64!");
                case 2:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_U2.cs->The size 2 should not exist, because it is always pushed as Int32 or Int64!");
				case 4:
						new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
						new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.AX, Size = 16 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
						break;
                case 8:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.AX, Size = 16 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U2: SourceSize " + xSource + " not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U2.cs->Unknown size of variable on the top of the stack.");
            }
			Assembler.Stack.Push(Align(4, 4), typeof(int));
        }
    }
}