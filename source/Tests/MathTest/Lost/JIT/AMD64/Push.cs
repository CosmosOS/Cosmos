using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Lost.JIT.AMD64
{
	public sealed class Push : ProcessorInstruction
	{
		public Push(GeneralPurposeRegister register)
		{
			Source = register;
		}
		public Push(MemoryOperand memory)
		{
			Source = memory;
		}
		public Push(ImmediateOperand immediate)
		{
			Source = immediate;
		}
		public Push(int value)
		{
			Source = new ImmediateOperand(value);
		}

		public InstructionOperand Source
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
			if (Source is MemoryOperand)
			{
				var dest = Source as MemoryOperand;
				Rex rex = NeedsRex(dest);
				if (rex != Rex.None) destStream.WriteByte((byte)rex);
				destStream.WriteByte(0xFF);
				WriteOperand(6, dest, destStream);
				return;
			}

			if (Source is GeneralPurposeRegister)
			{
				var dest = Source as GeneralPurposeRegister;
				if (dest.Size != 8) throw new NotSupportedException();
				if (dest.Register.IsNew()) destStream.WriteByte((byte)Rex.NewRegOpcode);
				destStream.WriteByte(0x50 + dest.Register.GetIndex());
				return;
				//destStream.WriteByte(ModRM(0, dest.Register, Registers.None));
			}

			if (Source is ImmediateOperand)
			{
				var source = Source as ImmediateOperand;
				if (source.Value.FitsInByte())
				{
					destStream.WriteByte(0x6A);
					destStream.WriteByte(source.Value);
				} else
				{
					destStream.WriteByte(0x68);
					destStream.WriteInt(source.Value);
				}
			}
		}

		public override string OpCodeFASM
		{
			get { return "push"; }
		}
		public override string ToFASM()
		{
			return string.Format(Source is MemoryOperand ? "push qword {0}" : "push {0}", Source);
		}
	}
}
