using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode( ILOpCode.Code.Neg )]
	public class Neg : ILOp
	{
		public Neg( Cosmos.Assembler.Assembler aAsmblr )
			: base( aAsmblr )
		{
		}

		public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
		{
			var xStackContent = aOpCode.StackPopTypes[0];
		    var xStackContentSize = SizeOfType(xStackContent);
		    var xStackContentIsFloat = TypeIsFloat(xStackContent);
			if (xStackContentSize > 4)
			{
				if (xStackContentIsFloat)
				{
					new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
					new CPUx86.x87.FloatNegate { };
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
				}
				else
				{
					new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX }; // low
					new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // high
					new CPUx86.Neg { DestinationReg = CPUx86.Registers.EBX }; // set carry if EBX != 0
					new CPUx86.AddWithCarry { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
					new CPUx86.Neg { DestinationReg = CPUx86.Registers.EAX };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
				}
			}
			else
			{
				if (xStackContentIsFloat)
				{
					new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, Size = 32, DestinationIsIndirect = true };
					new CPUx86.x87.FloatNegate { };
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 32, DestinationIsIndirect = true };
				}
				else
				{
					new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
					new CPUx86.Neg { DestinationReg = CPUx86.Registers.EAX };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
				}
			}
		}
	}
}
