using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "ja")]
	public class JumpIfAbove : JumpBase
	{
		/// <summary>
		/// <para>after CMP DEST, SOURCE</para>
		/// <para>if (DEST &gt; SOURCE) jump (unsigned)</para>
		/// </summary>
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