using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86
{
	/// <summary>
	/// Subtracts the source operand from the destination operand and 
	/// replaces the destination operand with the result. 
	/// </summary>
	[OpCode(0xFFFFFFFF, "sbb")]
	public class SubWithCarry : Instruction
	{
		public readonly string Destination, Source;

		public SubWithCarry(string aDestination, string aSource)
		{
			Destination = aDestination;
			Source = aSource;
		}

		public override string ToString()
		{
			return string.Format("sbb {0}, {1}", Destination, Source);
		}
	}
}
