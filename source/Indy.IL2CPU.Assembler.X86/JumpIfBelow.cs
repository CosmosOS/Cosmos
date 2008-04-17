using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	/// <summary>
	/// CMP DEST, SOURCE
	/// if (DEST &lt; SOURCE) jump (unsigned)
	/// </summary>
	[OpCode(0xFFFFFFFF, "jb")]
	public class JumpIfBelow : JumpBase
	{
		public JumpIfBelow(string aAddress)
			: base(aAddress)
		{
		}
		public override string ToString()
		{
			return "jb " + Address;
		}
	}
}