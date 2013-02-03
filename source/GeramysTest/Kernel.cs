using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace GeramysTest
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
            string input = Console.ReadLine();
            int i = 0;
            i++;
            i++;
            string test = "something";
            test += ", me.";
            Console.Write("Text typed: ");
            Console.WriteLine(input);
        }
    }
}
