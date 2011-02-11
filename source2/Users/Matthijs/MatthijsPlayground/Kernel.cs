using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace MatthijsPlayground
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.Write("Input: ");
            var input = Console.ReadLine();
            int xA = 0;
            int xB = 13;
            Console.WriteLine("Test");
            Console.WriteLine((xA + xB).ToString());
            //var xOldColor = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write("Text typed: ");
            //Console.WriteLine(input);
            //Console.ForegroundColor = xOldColor;
        }
    }
}
