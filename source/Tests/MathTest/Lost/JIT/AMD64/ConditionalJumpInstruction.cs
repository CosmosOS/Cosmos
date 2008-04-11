using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public abstract class ConditionalJumpInstruction: ProcessorInstruction
	{
		public ConditionalJumpInstruction(int targetOffset)
		{
			TargetOffset = targetOffset;
		}

		public int TargetOffset { get; set; }

		public override int? Size
		{
			get { throw new NotImplementedException(); }
		}

		public override void Compile(Stream destStream)
		{
			if (TargetOffset.FitsInSByte())
			{
				destStream.WriteByte(OpcodeBase);
				destStream.WriteSByte(TargetOffset);
				return;
			} else
			{
				destStream.WriteByte(0x0F);
				destStream.WriteByte(OpcodeBase + 0x10);
				destStream.WriteInt(TargetOffset);
			}
		}

		protected abstract int OpcodeBase { get; }

		public override string ToFASM()
		{
			return string.Format("{0} {1}", OpCodeFASM,
				TargetOffset + (TargetOffset.FitsInSByte() ? 2 : 6));
		}
	}
}
