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
#warning this instruction should be renamed to JumpIfGreater after corresponding obsolete instruction is deleted
	[OpCode(0xFFFFFFFF, "jg")]
	public class JumpOnGreater : JumpBase
	{
	}
}