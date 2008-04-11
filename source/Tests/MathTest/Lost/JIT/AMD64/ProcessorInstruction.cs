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
		public abstract string ToFASM();
		public abstract string OpCodeFASM { get; }
		public abstract void Compile(Stream destStream);

		public override string ToString()
		{
			return ToFASM();
		}

		public static Rex NeedsRex(MemoryOperand memory)
		{
			if (memory.RipBased) return Rex.None;
			if (memory.RequiresSIB())
			{
				Rex result = Rex.None;
				if ((memory.Base != null) && memory.Base.Register.IsNew()) result |= Rex.NewRegBase;
				if ((memory.Index != null) && memory.Index.Register.IsNew()) result |= Rex.NewRegIndex;
				return result;
			}
			if (memory.Index == null)
			{
				Rex result = Rex.None;
				if ((memory.Base != null) && memory.Base.Register.IsNew()) result |= Rex.NewRegBase;
				return result;
			}
			throw new NotImplementedException();
		}

		[Obsolete("Use WriteOperand(opcode_ext, dest, destStream)")] 
		public static byte ModRM(int ext, Registers dest)
		{
			int result = 0xC0;
			result += ext << 3;
			result += dest.GetIndex();
			return (byte)result;
		}
		[Obsolete("Use WriteOperands(reg, memory, destStream)")]
		public static void ModRM(Registers reg, int disp32, Stream destStream)
		{
			int result = 0;
			result += reg == Registers.None? 0: reg.GetIndex() << 3;
			result += 5;
			destStream.WriteByte(result);
			destStream.WriteInt(disp32);
		}
		[Obsolete("Use WriteOperand(opcode_ext, memory, destStream)")]
		public static void SIB(int opcode_ext, MemoryOperand memory, Stream destStream)
		{
			int result;
			switch (memory.DisplacementSize)
			{
			case 0:
				result = 0;
				break;
			case 1:
				result = 1;
				break;
			//case 2:
			//    result = 2;
			//    break;
			case 4:
				result = 2;
				break;
			default:
				throw new NotSupportedException("нет текста ошибки");
			}
			result <<= 6;
			result += opcode_ext << 3;
			result += 4;
			destStream.WriteByte(result);

			destStream.WriteByte(SIB(memory.Scale, memory.Index, memory.Base));

			switch (memory.DisplacementSize)
			{
			case 0:
				break;
			case 1:
				destStream.WriteByte(memory.Displacement);
				break;
			//case 2:
			//    destStream.WriteShort(disp);
			//    break;
			case 4:
				destStream.WriteInt(memory.Displacement);
				break;
			default:
				throw new NotSupportedException();
			}
		}
		[Obsolete("Use WriteOperands(reg, memory, destStream)")]
		public static void SIB(Registers reg, MemoryOperand memory, Stream destStream)
		{
			SIB(reg.GetIndex(), memory, destStream);
		}
		private static byte ModRM(int mode, int reg, int rm)
		{
			if (mode < 0 || mode >= 4) throw new ArgumentOutOfRangeException("mode", mode.ToString());
			if (reg < 0 || reg >= 8) throw new ArgumentOutOfRangeException("reg", reg.ToString());
			if (rm < 0 || rm >= 8) throw new ArgumentOutOfRangeException("rm", rm.ToString());

			return (byte)((mode << 6) + (reg << 3) + rm);
		}
		private static byte ModRM(int mode, Registers reg, Registers rm)
		{
			return ModRM(mode, reg.GetIndex(), rm.GetIndex());
		}
		private static byte ModRM(int mode, int opcode_ext, Registers rm)
		{
			return ModRM(mode, opcode_ext, rm.GetIndex());
		}
		private static byte SIB(int scale, GeneralPurposeRegister index, GeneralPurposeRegister base_reg)
		{
			if (scale.BitIndex() >= 4) throw new ArgumentOutOfRangeException("scale", scale.ToString());
			int result = scale.BitIndex() << 6;
			result += index == null? 4 << 3: index.Register.GetIndex() << 3;
			result += base_reg.Register.GetIndex();
			return checked((byte)result);
		}

		public static void WriteOperand(int opcode_ext, MemoryOperand memory, Stream destStream)
		{
			#region RIP-based
			if (memory.RipBased)
			{
				destStream.WriteByte(ModRM(0, opcode_ext, Binary(101)));
				destStream.WriteInt(memory.Displacement);
				return;
			}
			#endregion

			#region SIB
			if (memory.RequiresSIB())
			{
				if ((memory.Base != null) && (memory.Base.Register == Registers.SP))
						memory.Index = GeneralPurposeRegister.SP;

				int mode;
				switch (memory.DisplacementSize)
				{
				case 0:
					mode = 0;
					break;
				case 1:
					mode = 1;
					break;
				//case 2:
				//    result = 2;
				//    break;
				case 4:
					mode = 2;
					break;
				default:
					throw new NotSupportedException("нет текста ошибки");
				}
				destStream.WriteByte(ModRM(mode, opcode_ext, Binary(100)));

				destStream.WriteByte(SIB(memory.Scale, memory.Index, memory.Base));

				switch (memory.DisplacementSize)
				{
				case 0:
					break;
				case 1:
					destStream.WriteByte(memory.Displacement);
					break;
				//case 2:
				//    destStream.WriteShort(disp);
				//    break;
				case 4:
					destStream.WriteInt(memory.Displacement);
					break;
				default:
					throw new NotSupportedException();
				}
				return;
			}
			#endregion

			#region [REG]+disp
			if (memory.Index == null)
			{
				if (memory.Displacement == 0)
				{
					destStream.WriteByte(ModRM(0, opcode_ext, memory.Base.Register));
					return;
				}
				int mode = 0;
				switch (memory.DisplacementSize)
				{
				case 1:
					mode = Binary(01);
					break;
				case 4:
					mode = Binary(10);
					break;
				default:
					throw new NotImplementedException();
				}
				destStream.WriteByte(ModRM(mode, opcode_ext, memory.Base.Register));

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
			#endregion

			throw new NotImplementedException();
		}
		public static void WriteOperand(int opcode_ext, Registers rm, Stream destStream)
		{
			destStream.WriteByte(ModRM(3, opcode_ext, rm.GetIndex()));
		}
		public static void WriteOperands(Registers dest, Registers source, Stream destStream)
		{
			destStream.WriteByte(ModRM(3, dest, source));
		}
		public static void WriteOperands(GeneralPurposeRegister reg, MemoryOperand memory, Stream destStream)
		{
			WriteOperand(reg.Register.GetIndex(), memory, destStream);
		}
		static byte Binary(int value)
		{
			int result = 0;
			for (int bit = 0; value > 0; bit++, value /= 10)
				switch (value % 10)
				{
				case 0:
					break;
				case 1:
					result += 1 << bit;
					break;
				default:
					throw new ArgumentException("value");
				}
			return (byte)result;
		}

		protected static class MOD
		{
			/// <summary>
			/// регистр-регистр
			/// </summary>
			public const int RegReg = 0x3;
			/// <summary>
			/// Адресация без смещения
			/// </summary>
			public const int SBI = 0x00;
			/// <summary>
			/// Адресация с 8-ми битным смещением
			/// </summary>
			public const int D8 = 0x1;
			/// <summary>
			/// Адресация со смещением 16 или 32
			/// </summary>
			public const int DX = 0x2;
		}
		protected static class RM
		{
			/// <summary>
			/// Используется SIB если RM == UseSIB
			/// </summary>
			public const int UseSIB = 0x04;
		}
	}

	static class AMDAssemblerStreamExtensions
	{
		public static void WriteRex(this Stream stream, Rex value)
		{
			stream.WriteByte((byte)value);
		}
	}
}
