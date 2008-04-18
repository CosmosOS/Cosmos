using System;
using System.Collections.Generic;
using System.Text;

namespace TestSuite.Tests
{
    public class MathTest : TestBase
    {
        public override string Name
        {
            get { return "Math"; }
        }

        public override void Initialize()
        {

        }

        public override void Teardown()
        {
        }

        public override void Test()
        {
            Assert(1 + 1 == 2, "1 + 1 == 2");
            Assert(-1 + 2 == 1, "-1 + 2 == 1");
            Assert(2 * 2 == 4, "2 * 2 == 4");
            Assert(6 / 2 == 3, "6 / 2 == 3");
            Assert(5 - 2 == 3, "5 - 2 == 3");
			Assert(-1 < 1, "-1 < 1");
			Assert(0xFFFFFFFFu > 1u, "0xFFFFFFFFu > 1u");
            Assert(2 + 5 * 2 == 12, "2 + 5 * 2 == 12");
            Assert((2 + 5) * 2 == 14, "(2 + 5) * 2 == 14");
			long al = 0x1FFFFFFFF;
			long bl = 0x300000000;	//1L;
			al += 0x01;				//al == 0x200000000
			bl -= 0xFFFFFFFFL;		//bl == 0x200000001
			al -= 0x02;				//al == 0x1FFFFFFFE
			bl -= 0x03;				//bl == 0x1FFFFFFFE
			Assert(al == bl, "Int64 operations");
			Assert((-41L) - (-31L) == -10L, "Int64 negatives");
			Assert((0x1FFFFFFFFL & 0x100000000L) == 0x100000000L, "Int64 And");

            UInt32 a = 5;
            UInt32 b = 5;
            Assert(a == b, "UInt32 Equality");
            b = 10;
            Assert(a != b, "UInt32 Inequality");
			if (0xFFF00000002ul > 0xFF00000001ul) Console.WriteLine("UInt64 comprasion passed");
			if (0xFFF00000002ul == 0xFFF00000002ul) Console.WriteLine("Branch on Equality");

			#region bge
			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFF000000002L >= 0xFFFFFFF000000002L) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bge[e-] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFF000000002L >= 0xFFFFFFFFFFFFFFFFL) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bge[!-] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFFFFFFFFFFFL >= 0xFFFFFFF000000002L) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bge[g-] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0L >= -2L) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bge[g0>=-]");

			Console.WriteLine("");
			#endregion bge

			#region bge_un
			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFF000000002LU >= 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bge_un[e] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFF000000002LU >= 0xFFFFFFFFFFFFFFFFLU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bge_un[!] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFFFFFFFFFFFLU >= 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bge_un[g] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0x0000LU >= 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bge_un[g0>=\"-\"]");

			Console.WriteLine("");
			#endregion bge_un

			#region bgt
			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFF000000002L > 0xFFFFFFF000000002L) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bgt[e] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFF000000002L > 0xFFFFFFFFFFFFFFFFL) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bgt[!] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFFFFFFFFFFFL > 0xFFFFFFF000000002L) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bgt[g] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0L > -2L) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bgt[g0>\"-\"]");

			Console.WriteLine("");
			#endregion bgt

			#region bgt_un
			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFF000000002LU > 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bgt_un[e] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFF000000002LU > 0xFFFFFFFFFFFFFFFFLU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bgt_un[!] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFFFFFFFFFFFLU > 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bgt_un[g] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0x0000LU > 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bgt_un[g0>=\"-\"]");

			Console.WriteLine("");
			#endregion bgt_un

			#region ble
			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFF000000002L <= 0xFFFFFFF000000002L) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("ble[e-] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFF000000002L <= 0xFFFFFFFFFFFFFFFFL) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("ble[!-] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFFFFFFFFFFFL <= 0xFFFFFFF000000002L) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ble[g-] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0L <= -2L) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ble[g0>=-]");

			Console.WriteLine("");
			#endregion ble

			#region ble_un
			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFF000000002LU <= 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("ble_un[e] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFF000000002LU <= 0xFFFFFFFFFFFFFFFFLU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("ble_un[!] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFFFFFFFFFFFLU <= 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ble_un[g] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0x0000LU <= 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("ble_un[g0>=\"-\"]");

			Console.WriteLine("");
			#endregion ble_un

			#region blt
			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFF000000002L < 0xFFFFFFF000000002L) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("blt[e] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFF000000002L < 0xFFFFFFFFFFFFFFFFL) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("blt[!] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFFFFFFFFFFFL < 0xFFFFFFF000000002L) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("blt[g] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0L < -2L) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("blt[g0>\"-\"]");

			Console.WriteLine("");
			#endregion blt

			#region blt_un
			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFF000000002LU < 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("blt_un[e] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFFFFF000000002LU < 0xFFFFFFFFFFFFFFFFLU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("blt_un[!] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFFFFFFFFFFFFFFLU < 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("blt_un[g] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0x0000LU < 0xFFFFFFF000000002LU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("blt_un[g0>=\"-\"]");

			Console.WriteLine("");
			#endregion blt_un

			#region bne_un
			Console.ForegroundColor = ConsoleColor.White;
			if (0xFFFF000000000000LU != 0xFFFF000000000000LU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bne[he] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0xFFFF000100000000LU != 0xFFFF000000000000LU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bne[hne] ");

			Console.ForegroundColor = ConsoleColor.White;
			if (0x00000000FFFF0000LU != 0x00000000FFFF0000LU) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("bne[le] ");

			Console.ForegroundColor = ConsoleColor.Red;
			if (0x00000000FFFF0001LU != 0x00000000FFFF0000LU) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("bne[lne] ");

			Console.WriteLine("");
			#endregion

			bool codition;
			#region cgt
#warning this doesn't emit cgt => need another test
			codition = (1L > 2L) && true;
			Console.ForegroundColor = ConsoleColor.White;
			if (codition) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("cgt false  ");

			codition = (2L > 1L) && true;
			Console.ForegroundColor = ConsoleColor.Red;
			if (codition) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("cgt true");

			Console.WriteLine("");
			#endregion cgt

			#region cgt_un
			codition = (1LU > 2LU) && true;
			Console.ForegroundColor = ConsoleColor.White;
			if (codition) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("cgt_un false  ");

			codition = (2LU > 1LU) && true;
			Console.ForegroundColor = ConsoleColor.Red;
			if (codition) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("cgt_un true");

			Console.WriteLine("");
			#endregion cgt_un

			#region clt
			codition = (1L < 2L) && true;
			Console.ForegroundColor = ConsoleColor.Red;
			if (codition) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("clt false  ");

			codition = (2L < 1L) && true;
			Console.ForegroundColor = ConsoleColor.White;
			if (codition) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("clt true");

			Console.WriteLine("");
			#endregion clt

			#region clt_un
			codition = (1LU < 2LU) && true;
			Console.ForegroundColor = ConsoleColor.Red;
			if (codition) Console.ForegroundColor = ConsoleColor.White;
			Console.Write("clt_un false  ");

			codition = (2LU < 1LU) && true;
			Console.ForegroundColor = ConsoleColor.White;
			if (codition) Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("clt_un true");

			Console.WriteLine("");
			#endregion clt_un
		}
    }
}
