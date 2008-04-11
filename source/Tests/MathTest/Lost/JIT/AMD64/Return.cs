using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class Return: ProcessorInstruction
	{
		public Return()
		{
		}

		public Return(int popBytes)
		{
			PopBytes = popBytes;
		}

		public override int? Size
		{
			get { return PopBytes.HasValue ? 3 : 1; }
		}

		public int? PopBytes { get; set; }

		public override string ToFASM()
		{
			return PopBytes.HasValue ? string.Format("ret {0}", PopBytes.Value) : "ret";
		}

		public override string OpCodeFASM
		{
			get { return "ret"; }
		}

		public override void Compile(Stream destStream)
		{
			if (PopBytes.HasValue)
			{
				destStream.WriteByte(0xC2);
				destStream.WriteUShort(PopBytes.Value);
			} else
			{
				destStream.WriteByte(0xC3);
			}
		}
	}
}
