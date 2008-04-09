using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class JumpIfEqual: ConditionalJumpInstruction
	{
		public JumpIfEqual(int targetOffset) : base(targetOffset) { }

		protected override int OpcodeBase
		{
			get { return 0x74; }
		}
	}
}
