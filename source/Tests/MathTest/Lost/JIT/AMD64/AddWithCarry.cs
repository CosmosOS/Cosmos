using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class AddWithCarry: DestSourceInstruction
	{
		public AddWithCarry(InstructionOperand dest, InstructionOperand source): base(dest, source)
		{
		}

		public override void Compile(Stream dest)
		{
			throw new NotImplementedException();
		}
		public override int? Size
		{
			get { throw new NotImplementedException(); }
		}
	}
}
