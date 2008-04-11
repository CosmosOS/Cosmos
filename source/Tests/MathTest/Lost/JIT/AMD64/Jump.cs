using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class Jump: ProcessorInstruction
	{
		public Jump(int targetOffset)
		{
			Dest = new ImmediateOperand(targetOffset);
		}

		public InstructionOperand Dest { get; set; }

		public override int? Size
		{
			get { throw new NotImplementedException(); }
		}

		public override void Compile(Stream destStream)
		{
			if (Dest is ImmediateOperand)
			{
				var dest = Dest as ImmediateOperand;
				if (dest.Value.FitsInSByte())
				{
					destStream.WriteByte(0xEB);
					destStream.WriteSByte(dest.Value);
				} else
				{
					destStream.WriteByte(0xE9);
					destStream.WriteInt(dest.Value);
				}
				return;
			}

			throw new NotImplementedException();
		}

		public override string OpCodeFASM
		{
			get { return "jmp"; }
		}

		public override string ToFASM()
		{
			if (Dest is ImmediateOperand)
			{
				var dest = Dest as ImmediateOperand;
				return string.Format("jmp {0}", dest.Value + (dest.Value.FitsInSByte()? 2: 5)); 
			}

			throw new NotImplementedException();
		}
	}
}
