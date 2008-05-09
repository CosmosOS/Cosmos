using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public class Label: ProcessorInstruction
	{
		public string Name { get; set; }

		public override int? Size
		{
			get { return 0; }
		}

		public override string ToFASM()
		{
			return string.Format("{0}: ", Name);
		}

		public override string OpCodeFASM
		{
			get { return ":"; }
		}

		public override void Compile(Stream destStream)
		{
			
		}
	}
}
