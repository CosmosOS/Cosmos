using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public abstract class DestSourceInstruction: ProcessorInstruction
	{
		public DestSourceInstruction(InstructionOperand dest, InstructionOperand source)
		{
			this.Dest = dest;
			this.Source = source;
		}
		public DestSourceInstruction(InstructionOperand dest, byte imm)
		{
			this.Dest = dest;
			this.Source = new ImmediateOperand(imm);
		}
		public DestSourceInstruction(InstructionOperand dest, short imm)
		{
			this.Dest = dest;
			this.Source = new ImmediateOperand(imm);
		}
		public DestSourceInstruction(InstructionOperand dest, int imm)
		{
			this.Dest = dest;
			this.Source = new ImmediateOperand(imm);
		}
		public DestSourceInstruction(InstructionOperand dest, long imm)
		{
			this.Dest = dest;
			this.Source = new ImmediateOperand(imm);
		}


		public InstructionOperand Dest { get; private set; }
		public InstructionOperand Source { get; private set; }

		public override void Compile(Stream destStream)
		{
			if (Source is ImmediateOperand)
			{
				var source = Source as ImmediateOperand;
				if (Dest is GeneralPurposeRegister)
				{
					var dest = Dest as GeneralPurposeRegister;
					#region INSTR accum, imm
					if (dest.Register == Registers.AX && (dest.Size == source.Size
						|| (dest.Size == 8 && source.Size == 4)))
					{
						switch (dest.Size)
						{
						case 1:
							destStream.WriteByte(AccumulatorOpCode);
							destStream.WriteByte(source.Value);
							return;
						case 2:
							destStream.WriteByte(OperandSizeOverride);
							destStream.WriteByte(AccumulatorOpCode + 1);
							destStream.WriteShort(source.Value);
							return;
						case 4:
							destStream.WriteByte(AccumulatorOpCode + 1);
							destStream.WriteInt(source.Value);
							return;
						case 8:
							destStream.WriteByte((byte)Rex.Wide);
							destStream.WriteByte(AccumulatorOpCode + 1);
							destStream.WriteInt(source.Value);
							return;
						}
						throw new NotSupportedException("Supported operand sizes are 1, 2, 4, and 8 bytes");
					}
					#endregion

					Rex rex = Rex.None;
					if ((dest.Register & Registers.NewRegsMask) != Registers.None) rex |= Rex.Reg;

					#region INSTR regX, immX //reg64, imm32
					if (dest.Size == source.Size || (dest.Size == 8 && source.Size == 4))
					{
						switch (dest.Size)
						{
						case 1:
							if (rex != Rex.None) destStream.WriteByte((byte)rex);
							destStream.WriteByte(ImmediateOpCode);
							destStream.WriteByte(ModRM(ImmediateExt, dest.Register));
							destStream.WriteByte(source.Value);
							return;
						case 2:
							destStream.WriteByte(OperandSizeOverride);
							if (rex != Rex.None) destStream.WriteByte((byte)rex);
							destStream.WriteByte(ImmediateOpCode + 1);
							destStream.WriteByte(ModRM(ImmediateExt, dest.Register));
							destStream.WriteShort(source.Value);
							return;
						case 4:
							if (rex != Rex.None) destStream.WriteByte((byte)rex);
							destStream.WriteByte(ImmediateOpCode + 1);
							destStream.WriteByte(ModRM(ImmediateExt, dest.Register));
							destStream.WriteInt(source.Value);
							return;
						case 8:
							rex |= Rex.Wide;
							destStream.WriteByte((byte)rex);
							destStream.WriteByte(ImmediateOpCode + 1);
							destStream.WriteByte(ModRM(ImmediateExt, dest.Register));
							destStream.WriteInt(source.Value);
							return;
						}
						throw new NotImplementedException("Supported operand sizes are 1, 2, 4, and 8 bytes");
					}
					#endregion

					#region INST reg, imm8
					if (source.Size == 1)
					{
						switch (dest.Size)
						{
						case 2:
							destStream.WriteByte(OperandSizeOverride);
							goto case 4;
						case 4:
							if (rex != Rex.None) destStream.WriteByte((byte)rex);
							destStream.WriteByte(ImmediateOpCode + 3);
							destStream.WriteByte(ModRM(ImmediateExt, dest.Register));
							destStream.WriteByte(source.Value);
							return;
						case 8:
							rex |= Rex.Wide;
							goto case 4;
						}
						throw new NotSupportedException();
					}
					#endregion
				}

			}
			throw new NotImplementedException();
		}
		public override int? Size
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// INSTR	accum, imm
		/// </summary>
		public abstract byte AccumulatorOpCode { get; }
		/// <summary>
		/// INSTR	reg/mem, imm
		/// </summary>
		public abstract byte ImmediateOpCode { get; }
		/// <summary>
		/// INSTR	reg/mem, imm
		/// </summary>
		public abstract byte ImmediateExt { get; }
		/// <summary>
		/// INSTR	reg, reg/mem
		/// </summary>
		public abstract byte RegisterOpCode { get; }
	}
}
