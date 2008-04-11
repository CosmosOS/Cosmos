using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public class Move: ProcessorInstruction
	{
		public Move(InstructionOperand dest, InstructionOperand source)
		{
			Dest = dest;
			Source = source;
		}

		public InstructionOperand Dest { get; set; }
		public InstructionOperand Source { get; set; }

		public override int? Size
		{
			get { throw new NotImplementedException(); }
		}

		public override string ToFASM()
		{
			if ((Dest is GeneralPurposeRegister && Source is GeneralPurposeRegister)
				//||
				) return string.Format("mov {0}, {1}", Dest, Source);

			throw new NotImplementedException();
		}

		public override string OpCodeFASM
		{
			get { return "mov"; }
		}

		public override void Compile(Stream destStream)
		{
			GeneralPurposeRegister reg;
			Rex rex = Rex.None;

			#region Source is Immediate
			if (Source is ImmediateOperand)
			{
				var source = Source as ImmediateOperand;
				if (Dest is GeneralPurposeRegister)
				{
					reg = Dest as GeneralPurposeRegister;
					int opcode = 0xB0;
					opcode += reg.Register.GetIndex();
					if (reg.Register.IsNew()) rex |= Rex.Reg;
					switch (reg.Size)
					{
					case 1:
						if (rex != Rex.None) destStream.WriteRex(rex);
						destStream.WriteByte(opcode);
						destStream.WriteByte(source.Value);
						return;
					case 2:
						destStream.WriteByte(OperandSizeOverride);
						if (rex != Rex.None) destStream.WriteRex(rex);
						destStream.WriteByte(opcode + 8);
						destStream.WriteShort(source.Value);
						return;
					case 4:
						if (rex != Rex.None) destStream.WriteRex(rex);
						destStream.WriteByte(opcode + 8);
						destStream.WriteInt(source.Value);
						return;
					case 8:
						rex |= Rex.Wide;
						if (rex != Rex.None) destStream.WriteRex(rex);
						destStream.WriteByte(opcode + 8);
						destStream.WriteLong(source.Value);
						return;
					default:
						throw new NotSupportedException();
					}
				}

				if (Dest is MemoryOperand)
				{
					var mem = Dest as MemoryOperand;
					throw new NotImplementedException();
				}
			}
			#endregion Source is Immediate

			int opcode_base = 0x88;
			if (Dest is GeneralPurposeRegister && Source is GeneralPurposeRegister)
			{
				var dest = Dest as GeneralPurposeRegister;
				var source = Source as GeneralPurposeRegister;
				if (dest.Size != source.Size) throw new NotSupportedException();
				if (dest.Register.IsNew()) rex |= Rex.NewRegRM;
				if (source.Register.IsNew()) rex |= Rex.Reg;
				switch (dest.Size)
				{
				case 1:
					break;
				case 2:
					destStream.WriteByte(OperandSizeOverride);
					goto case 4;
				case 4:
					opcode_base++;
					break;
				case 8:
					rex |= Rex.Wide;
					goto case 4;
				default:
					throw new NotSupportedException();
				}
				if (rex != Rex.None) destStream.WriteRex(rex);
				destStream.WriteByte(opcode_base);
				WriteOperands(source.Register, dest.Register, destStream);
				return;
			}
			var memory = Dest is GeneralPurposeRegister ? Source as MemoryOperand : Dest as MemoryOperand;
			reg = Dest is GeneralPurposeRegister ? Dest as GeneralPurposeRegister : Source as GeneralPurposeRegister;
			if (Dest is GeneralPurposeRegister) opcode_base += 2;

			#region Rip-Relative Addressing
			rex = NeedsRex(memory);
			if (reg.Register.IsNew()) rex |= Rex.Reg;
			#endregion Rip-Relative Addressing
		}
	}
}
