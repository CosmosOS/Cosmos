using System;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;
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
					new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
					new CPUx86.x87.FloatNegate { };
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
				}
				else
				{
					XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX)); // low
					XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX)); // high
					XS.Negate(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX)); // set carry if EBX != 0
					new CPUx86.AddWithCarry { DestinationReg = CPUx86.RegistersEnum.EAX, SourceValue = 0 };
					XS.Negate(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX));
				}
			}
			else
			{
				if (xStackContentIsFloat)
				{
					new FloatLoad { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 32, DestinationIsIndirect = true };
					new FloatNegate { };
					new FloatStoreAndPop { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 32, DestinationIsIndirect = true };
				}
				else
				{
					XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Negate(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
				}
			}
		}
	}
}
