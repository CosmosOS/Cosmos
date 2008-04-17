using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	/// <summary>
	/// CMP DEST, SOURCE
	/// if (DEST > SOURCE) jump (signed)
	/// </summary>
	[OpCode(0xFFFFFFFF, "jg")]
	[Obsolete("This instruction's generated asm code differs from it's name. Use JumpOnGreater instead.")]
	public class JumpOnGreater : JumpBase
	{
		public JumpOnGreater(string aAddress)
			: base(aAddress)
		{
		}
		public override string ToString()
		{
			return "jg " + Address;
		}
	}
}