using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class Call: ProcessorInstruction
	{
		public Call(int targetOffset)
		{
			Dest = new ImmediateOperand(targetOffset);
		}
		public Call(GeneralPurposeRegister targetAddress)
		{
			Dest = targetAddress;
		}
		public Call(MemoryOperand targetAddress)
		{
			Dest = targetAddress;
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

			if (Dest is GeneralPurposeRegister)
			{
				var dest = Dest as GeneralPurposeRegister;
				if (dest.Size != 8) throw new InvalidProgramException();
				if (dest.Register.IsNew()) destStream.WriteRex(Rex.Reg);
				destStream.WriteByte(0xFF);
				WriteOperand(2, dest.Register, destStream);
				return;
			}

			if (Dest is MemoryOperand)
			{
				var dest = Dest as MemoryOperand;
				Rex rex = NeedsRex(dest);
				if (rex != Rex.None) destStream.WriteRex(rex);
				destStream.WriteByte(0xFF);
				WriteOperand(2, dest, destStream);
				return;
			}

			throw new NotImplementedException();
		}

		public override string OpCodeFASM
		{
			get { return "call"; }
		}

		public override string ToFASM()
		{
			return string.Format(Dest is MemoryOperand? "call qword {0}": "call {0}", Dest);
		}
	}
}
