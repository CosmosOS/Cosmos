using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Assembler.Assembler;
using CPUAll = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.Plugs.CustomImplementations.MS.System
{
	[Plug(Target = typeof(global::System.Math))]
	class MathImpl
	{
		#region double Sqrt(double d)
		public class CalculateSqrtAsm : AssemblerMethod
		{
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
			{
				new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.EBP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
				new CPUx86.x87.FloatSqrt{ };
				// reservate 8 byte for returntype double on stack
				new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
				// write double value to this reservation
				new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ESP, Size = 64, DestinationIsIndirect = true };
				// after this is the result popped
			}
		}
		[PlugMethod(Assembler = typeof(CalculateSqrtAsm))]
		public static double Sqrt(double d) { return 0.0; }
		#endregion
	}
}