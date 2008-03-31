using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public abstract class ProcessorInstruction
	{
		public abstract int? Size { get; }
		public abstract void Compile(Stream dest);
	}
}
