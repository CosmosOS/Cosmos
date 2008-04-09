using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Lost.JIT.AMD64
{
	public sealed class Pop: ProcessorInstruction
	{
		public Pop(GeneralPurposeRegister register)
		{
			Dest = register;
		}
		public Pop(MemoryOperand memory)
		{
			Dest = memory;
		}

		public InstructionOperand Dest
		{
			get;
			set;
		}

		public override int? Size
		{
			get { throw new NotImplementedException(); }
		}

		public override void Compile(Stream destStream)
		{
			if (Dest is MemoryOperand)
			{
				var dest = Dest as MemoryOperand;
				Rex rex = NeedsRex(dest);
				if (rex != Rex.None) destStream.WriteByte((byte)rex);
				destStream.WriteByte(0x8F);
				WriteOperand(0, dest, destStream);
				return;
			}

			if (Dest is GeneralPurposeRegister)
			{
				var dest = Dest as GeneralPurposeRegister;
				if (dest.Size != 8) throw new NotSupportedException();
				if (dest.Register.IsNew()) destStream.WriteByte((byte)Rex.NewRegOpcode);
				destStream.WriteByte(0x58 + dest.Register.GetIndex());
				return;
				//destStream.WriteByte(ModRM(0, dest.Register, Registers.None));
			}
			throw new InvalidProgramException();
		}
	}
}
