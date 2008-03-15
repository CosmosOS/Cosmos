using System;
using System.Collections.Generic;
using System.Text;

namespace MathTest
{
	class LongArithmetics
	{
		public static void Test()
		{
			Console.WriteLine("+++LongArithmeticsTest+++");

			Console.Write("carry test...");
			long a = 0x1FFFFFFFFL;
			long b = 1;

			if (a + b != 0x200000000L)
			{
				Console.WriteLine("failed");
				Console.Write("excepted: ");
				Console.WriteLine((0x200000000L).ToString());
				Console.Write("received: ");
				Console.WriteLine((a + b).ToString());
			} else
				Console.WriteLine("passed");


			Console.WriteLine("");
		}
	}
}
