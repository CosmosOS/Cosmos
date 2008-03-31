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
		public const byte OperandSizeOverride = 0x66;
		public const byte AddressSizeOverride = 0x67;

		public abstract int? Size { get; }
		public abstract void Compile(Stream destStream);

		public static byte ModRM(int mod, int reg, Registers rm)
		{
			mod <<= 6;
			mod += reg << 3;
			mod += rm.GetIndex();
			return (byte)mod;
		}
	}
}
