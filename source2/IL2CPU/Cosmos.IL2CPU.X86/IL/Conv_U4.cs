using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to UInt32 and change its type to Int32.
	/// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_U4 )]
    public class Conv_U4 : ILOp
    {
        public Conv_U4( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			var xSource = Assembler.Stack.Pop();
            switch( xSource.Size )
            {
                case 1:
                case 2:
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Conv_U4.cs->The size {0:D} could not exist, because always is pushed Int32 or Int64!");
				case 4:
					if (xSource.IsFloat)
					{
						new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
						new CPUx86.SSE.ConvertSS2SIAndTruncate { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.XMM0, };
						new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
					}
					break;
                case 8:
					if (xSource.IsFloat)
					{
                        new CPUx86.SSE.MoveDoubleAndDupplicate { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
						new CPUx86.SSE.ConvertSD2SIAndTruncate { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.XMM0, };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
					}
					else
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U4: SourceSize " + xStackItem.Size + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U4.cs->Unknown size of variable on the top of the stack.");
            }
            Assembler.Stack.Push(Align(4, 4), typeof(Int32));
        }
    }
}