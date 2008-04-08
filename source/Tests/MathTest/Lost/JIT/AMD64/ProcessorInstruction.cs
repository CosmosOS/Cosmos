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
		public static void SIB(Registers reg, int scale, Registers index, Registers base_reg, Stream destStream, int disp, int disp_size)
		{
			int result = 0;
			switch (disp_size)
			{
			case 1:
				result = 1;
			case 2:
				result = 2;
			case 4:
				result = 3;
			default:
				throw new NotSupportedException("нет текста ошибки");
			}
			result <<= 6;
			result += reg.GetIndex() << 3;
			result += 5;
			destStream.WriteByte(result);

			destStream.WriteByte(SIB(scale, index, base_reg));

			throw new NotImplementedException();
		}
		public static byte SIB(int scale, Registers index, Registers base_reg)
		{
			throw new NotImplementedException();
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
