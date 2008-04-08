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
}
