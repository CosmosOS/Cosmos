using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.Windows;

namespace CipherTest
{
	class Program
	{
		public Program()
		{
			
		}

		public Program(int i)
		{
			delegatevar = i;
		}

		private int delegatevar = 0;

		public int InstanceReturn()
		{
			return delegatevar;
		}

		public void InstancePrint()
		{
			Console.WriteLine(delegatevar);
		}

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
			Console.WriteLine("i*4=" + x);
			//return x;
		}

		public static void Times2(int i, int t)
		{
			Console.WriteLine("Two Argument Test");
			Console.WriteLine("i=" + i.ToString() + ";t=" + t.ToString());
			int x = i * 2;
			Console.WriteLine("i*2=" + x.ToString());
		}

		public static void VarArgs(params int[] i)
		{
			Console.WriteLine("Var Argument Test -- just like one arg");
			foreach (var item in i)
				Console.WriteLine("item=" + item.ToString());

		}

		public delegate void Test2Args(int i, int t);
		public delegate void Test1Args(int i);
		public delegate void TestVarArgs(params int[] i);

		public delegate void TestNoArgs();

		public delegate int TestReturn();

		public static void Init()
		{
			Cosmos.Sys.Boot.Default();
			Console.WriteLine("Boot complete");
			Console.WriteLine("Press a key to test delegates!");
			Console.ReadLine();
			Test2Args testfor2args = Times2;
			testfor2args(4, 3);
			testfor2args += Times2;
			testfor2args(4, 3);
			Test1Args testFor1Arg = Times4;
			testFor1Arg(4);
			TestNoArgs testNoArgs = () => Console.WriteLine("NoArgs");
			testNoArgs();
			var p = new Program(0xfeed);
			Console.WriteLine("Instance call");
			testNoArgs = p.InstancePrint;
			Console.WriteLine("Test Returning delegates");
			Console.ReadLine();
			TestReturn testForReturnvalues = return2;
			int i = testForReturnvalues();
			Console.WriteLine(i);
			testForReturnvalues += return3;
			i = testForReturnvalues();
			Console.WriteLine(i);
			Console.WriteLine("Instace Return");
			testForReturnvalues = p.InstanceReturn;
			i = testForReturnvalues();
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
			return 0xdead;
		}
		public static int return3()
		{
			return 3;
		}
	}
}
