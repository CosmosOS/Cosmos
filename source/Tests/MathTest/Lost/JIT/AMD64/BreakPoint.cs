using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class BreakPoint: ProcessorInstruction
	{
		public override int? Size
		{
			get { return 1; }
		}

		public override void Compile(Stream destStream)
		{
			destStream.WriteByte(0xCC);
		}

		public override string OpCodeFASM
		{
			get { return "int 3"; }
		}
		public override string ToFASM()
		{
			return OpCodeFASM;
		}
	}
}
