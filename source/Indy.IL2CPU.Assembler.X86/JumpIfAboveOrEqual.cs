using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "jae")]
	public class JumpIfAboveOrEqual : JumpBase
	{
		/// <summary>
		/// <para>after CMP DEST, SOURCE</para>
		/// <para>if (DEST &gt;= SOURCE) jump (unsigned)</para>
		/// </summary>
		public JumpIfAboveOrEqual(string aAddress)
			: base(aAddress)
		{
		}
		public override string ToString()
		{
			return "jae " + Address;
		}
	}
}
