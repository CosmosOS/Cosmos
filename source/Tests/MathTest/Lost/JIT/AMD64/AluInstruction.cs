using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public abstract class AluInstruction: ProcessorInstruction
	{
		public AluInstruction(InstructionOperand dest, InstructionOperand source)
		{
			this.Dest = dest;
			this.Source = source;
		}
		public AluInstruction(InstructionOperand dest, byte imm)
		{
			this.Dest = dest;
			this.Source = new ImmediateOperand(imm);
		}
		public AluInstruction(InstructionOperand dest, short imm)
		{
			this.Dest = dest;
			this.Source = new ImmediateOperand(imm);
		}
		public AluInstruction(InstructionOperand dest, int imm)
		{
			this.Dest = dest;
			this.Source = new ImmediateOperand(imm);
		}
		public AluInstruction(InstructionOperand dest, long imm)
		{
			this.Dest = dest;
			this.Source = new ImmediateOperand(imm);
		}


		public InstructionOperand Dest { get; private set; }
		public InstructionOperand Source { get; private set; }

		public override void Compile(Stream destStream)
		{
			#region INSTR dest, imm
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
					if (dest.Register.IsNew()) rex |= Rex.NewRegRM;

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

					throw new NotSupportedException();
				}

			}
			#endregion

			int opcode_base = RegisterOpCode;
			#region OP regX, regX
			if (Dest is GeneralPurposeRegister && Source is GeneralPurposeRegister)
			{
				var dest = Dest as GeneralPurposeRegister;
				var source = Source as GeneralPurposeRegister;
				if (dest.Size != source.Size) throw new NotSupportedException();
				Rex rex = Rex.None;
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
			#endregion OP regX, regX

			#region OP memX/regX, regX (OP regX, memX/regX)
			var memory = Dest is GeneralPurposeRegister ? Source as MemoryOperand : Dest as MemoryOperand;
			var reg = Dest is GeneralPurposeRegister ? Dest as GeneralPurposeRegister : Source as GeneralPurposeRegister;
			if (Dest is GeneralPurposeRegister) opcode_base += 2;

			#region RIP-Relative addressing
			if (memory.RipBased)
			{
				Rex rex = Rex.None;
				if (reg.Register.IsNew()) rex |= Rex.Reg;
				switch (reg.Size)
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
				if (rex != Rex.None) destStream.WriteByte((byte)rex);
				destStream.WriteByte(opcode_base);
				ModRM(reg.Register, memory.Displacement, destStream);
				return;
			}
			#endregion

			#region SIB addressing
			if (memory.RequiresSIB())
			{
				Rex rex = Rex.None;
				if (reg.Register.IsNew()) rex |= Rex.Reg;
				if ((memory.Base != null) && memory.Base.Register.IsNew()) rex |= Rex.B;

				if ((memory.Base != null) && (memory.Base.Register == Registers.SP))
					memory.Index = GeneralPurposeRegister.SP;

				if ((memory.Index != null) && memory.Index.Register.IsNew()) rex |= Rex.NewRegIndex;
				switch (reg.Size)
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
				if (rex != Rex.None) destStream.WriteByte((byte)rex);
				destStream.WriteByte(opcode_base);
				SIB(reg.Register, memory, destStream);
				return;
			}
			#endregion SIB addressing

			#region mem is [reg]
			if (memory.Index == null)
			{
				Rex rex = Rex.None;
				if (reg.Register.IsNew()) rex |= Rex.Reg;
				if (memory.Base.Register.IsNew()) rex |= Rex.B;
				switch (reg.Size)
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
				if (rex != Rex.None) destStream.WriteByte((byte)rex);
				destStream.WriteByte(opcode_base);
				int modRM = 0;
				if (memory.Displacement == 0)
				{
					modRM += reg.Register.GetIndex() << 3;
					modRM += memory.Base.Register.GetIndex();
					destStream.WriteByte(modRM);
					return;
				}
				switch (memory.DisplacementSize)
				{
				case 1:
					modRM = 1 << 6;
					break;
				case 4:
					modRM = 2 << 6;
					break;
				default:
					throw new NotImplementedException();
				}
				modRM += reg.Register.GetIndex() << 3;
				modRM += memory.Base.Register.GetIndex();
				destStream.WriteByte(modRM);
				switch (memory.DisplacementSize)
				{
				case 1:
					destStream.WriteByte(memory.Displacement);
					break;
				case 4:
					destStream.WriteInt(memory.Displacement);
					break;
				default:
					throw new NotImplementedException();
				}

				return;
			}
			#endregion mem is [reg]
			#endregion OP memX/regX, regX (OP regX, memX/regX)

			throw new NotImplementedException();
		}
		public override int? Size
		{
			get { throw new NotImplementedException(); }
		}

		public override string ToFASM()
		{
			return string.Format("{0} {1}, {2}", OpCodeFASM, Dest, Source);
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
