using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	/// <summary>
	/// CMP DEST, SOURCE
	/// if (DEST &gt; SOURCE) jump (unsigned)
	/// </summary>
	[OpCode(0xFFFFFFFF, "ja")]
	public class JumpIfAbove : JumpBase
	{
		public JumpIfAbove(string aAddress)
			: base(aAddress)
		{
		}
		public override string ToString()
		{
			return "ja " + Address;
		}
	}
}