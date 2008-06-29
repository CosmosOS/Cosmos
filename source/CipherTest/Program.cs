using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.Windows;

namespace CipherTest
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			// This is here to run it on Windows and see results when necessary
			// Then can be run on Cosmos to see if values are the same
			//RTL8139.CreateTestFrame();
			BuildUI.Run();
		}

		public static void Times4(int i)
		{
			Console.WriteLine("One Argument Test");
			Console.WriteLine("i=" + i.ToString());
			int x = i * 4;
			Console.WriteLine("i*4="+x);
			//return x;
		}

		public static void Times2(int i,int t)
		{
			Console.WriteLine("Two Argument Test");
			Console.WriteLine("i=" + i.ToString() + ";t=" + t.ToString());
			int x = i * 2;
			Console.WriteLine("i*2="+x.ToString());
		}

		public delegate void Test2Args(int i,int t);
		public delegate void Test1Args(int i);

		public delegate void TestNoArgs();

		public delegate int TestReturn();

		public static void Init()
		{
			Cosmos.Sys.Boot.Default();
			Console.WriteLine("Boot complete");
			Console.WriteLine("Press a key to test delegates!");
			Test2Args testfor2args = Times2;
			testfor2args(4, 3);
			Test1Args testFor1Arg = Times4;
			testFor1Arg(4);
			TestNoArgs testNoArgs = () => Console.WriteLine("NoArgs");
			testNoArgs();
			TestReturn testForReturnvalues = return2;
			int i=testForReturnvalues();
			Console.WriteLine(i);
			if (Console.ReadLine() == "r")
			{
				Console.WriteLine("Rebooting...");
				Cosmos.Sys.Deboot.Reboot();
			}
			else
				Cosmos.Sys.Deboot.Halt();
		}
		public static int return2()
		{
			return 2;
		}
		public static int return3(int i)
		{
			return 3;
		}
	}
}
