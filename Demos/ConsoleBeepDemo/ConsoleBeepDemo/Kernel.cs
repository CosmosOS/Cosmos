using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace ConsoleBeepDemo
{
    public class Kernel: Sys.Kernel
    {
		protected override void BeforeRun()
		{
			Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
			
		}
        
        protected override void Run()
        {
			Console.WriteLine("Run Mary Had a Little Lamb?");
			string ans = Console.ReadLine();
			if (ans.ToLower() == "y" || ans.ToLower() == "yes")
			{
				Test.Main();
			}
			else
			{
				Console.WriteLine("Default beep:");
				Console.Beep();
				// Does the follwing: Console.Beep((int)Sys.Notes.Default (800 hertz), (int)Sys.Durations.Default (200 milliseconds) );
			}
		}
    }
}
