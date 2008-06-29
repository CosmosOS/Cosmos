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
			Console.WriteLine("i=" + i.ToString());
			int x = i * 4;
			Console.WriteLine(x);
			//return x;
		}

		public static void Times2(int i,int t)
		{
			Console.WriteLine("i=" + i.ToString() + ";t=" + t.ToString());
			int x = i * 2;
			Console.WriteLine(x);
		}

		public delegate void Test(int i);

		public static void Init()
		{
			Cosmos.Sys.Boot.Default();
			Console.WriteLine("Boot complete");
			Console.WriteLine("Press a key to test delegates!");
			Test t = Times4;
			//int x=
			t(4);
			//t += Times2;
			//t(4,3);
			//Console.WriteLine(x);
			if (Console.ReadLine() == "r")
			{
				Console.WriteLine("Rebooting...");
				Cosmos.Sys.Deboot.Reboot();
			}
			else
				Cosmos.Sys.Deboot.Halt();
		}
		public static int return2(int i)
		{
			return 2;
		}
		public static int return3(int i)
		{
			return 3;
		}
	}
}
