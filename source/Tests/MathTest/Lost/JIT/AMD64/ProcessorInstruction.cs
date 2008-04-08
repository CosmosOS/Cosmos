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
		public abstract void Compile(Stream destStream);

		public static byte ModRM(int ext, Registers dest)
		{
			int result = 0xC0;
			result += ext << 3;
			result += dest.GetIndex();
			return (byte)result;
		}
		public static void Write(Registers reg, Registers rm, Stream destStream)
		{
			int result = 0xC0;
			result += reg.GetIndex() << 3;
			result += rm.GetIndex();
			destStream.WriteByte(result);
		}
		public static void ModRM(Registers reg, int disp32, Stream destStream)
		{
			int result = 0;
			result += reg.GetIndex() << 3;
			result += 5;
			destStream.WriteByte(result);
			destStream.WriteInt(disp32);
		}
		public static void SIB(Registers reg, int scale, GeneralPurposeRegister index, GeneralPurposeRegister base_reg, int disp, int disp_size, Stream destStream)
		{
			int result;
			switch (disp_size)
			{
			case 0:
				result = 0;
				break;
			case 1:
				result = 1;
				break;
			case 2:
				result = 2;
				disp_size = 4;
				break;
			case 4:
				result = 2;
				break;
			default:
				throw new NotSupportedException("нет текста ошибки");
			}
			result <<= 6;
			result += reg.GetIndex() << 3;
			result += 4;
			destStream.WriteByte(result);

			destStream.WriteByte(SIB(scale, index, base_reg));

			switch (disp_size)
			{
			case 0: 
				break;
			case 1:
				destStream.WriteByte(disp);
				break;
			case 2:
				destStream.WriteShort(disp);
				break;
			case 4:
				destStream.WriteInt(disp);
				break;
			default:
				throw new NotSupportedException();
			}
		}
		public static byte SIB(int scale, GeneralPurposeRegister index, GeneralPurposeRegister base_reg)
		{
			int result = scale.BitIndex() << 6;
			result += index.Register.GetIndex() << 3;
			result += base_reg.Register.GetIndex();
			return checked((byte)result);
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
}
