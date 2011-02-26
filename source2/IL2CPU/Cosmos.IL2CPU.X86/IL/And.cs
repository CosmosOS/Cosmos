using System;
using Cosmos.IL2CPU.X86;
using CPU = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode( ILOpCode.Code.And )]
	public class And : ILOp
	{
		public And( Cosmos.Compiler.Assembler.Assembler aAsmblr )
			: base( aAsmblr )
		{
		}

		public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
		{
			var xStackContent = Assembler.Stack.Pop();
			var xStackContentSecond = Assembler.Stack.Pop();

			var xSize = Math.Max( xStackContent.Size, xStackContentSecond.Size);
			if (ILOp.Align(xStackContent.Size, 4u) != ILOp.Align(xStackContentSecond.Size, 4u))
				throw new NotSupportedException("Operands have different size!");
			if( xSize > 8 )
				throw new NotImplementedException( "StackSize>8 not supported" );

			if( xSize > 4 )
			{
				// [ESP] is low part
				// [ESP + 4] is high part
				// [ESP + 8] is low part
				// [ESP + 12] is high part
				new CPU.Pop { DestinationReg = CPU.Registers.EAX };
				new CPU.Pop { DestinationReg = CPU.Registers.EDX };
				// [ESP] is low part
				// [ESP + 4] is high part
				new CPU.And { DestinationReg = CPU.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPU.Registers.EAX };
				new CPU.And { DestinationReg = CPU.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPU.Registers.EDX };
			}
			else
			{
                new CPU.Pop { DestinationReg = CPU.Registers.EAX };
                new CPU.And { DestinationReg = CPU.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPU.Registers.EAX };
			}
			Assembler.Stack.Push( xStackContent );
		}
	}
}