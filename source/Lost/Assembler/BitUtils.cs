using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost
{
	public static class BitUtils
	{
		public static int BitIndex(this int value)
		{
			for (int i = 0; i < 32; i++)
				if (value == 1 << i) return i;
			throw new ArgumentException("this");
		}
	}

	public static class IntUtils
	{
		public static bool FitsInByte(this int value)
		{
			return ((byte)value) == value;
		}
		public static bool FitsInByte(this long value)
		{
			return ((byte)value) == value;
		}
		public static bool FitsInSByte(this long value)
		{
			return ((sbyte)value) == value;
		}
		public static bool FitsInSByte(this int value)
		{
			return ((sbyte)value) == value;
		}
	}
}
