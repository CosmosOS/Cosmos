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

		public static void Init()
		{
			Cosmos.Sys.Boot.Default();
			Console.WriteLine("Boot complete");
			if (Console.ReadLine() == "r")
			{
				Console.WriteLine("Rebooting...");
				Cosmos.Sys.Deboot.Reboot();
			}
			else
				Cosmos.Sys.Deboot.Halt();
		}
	}
}
