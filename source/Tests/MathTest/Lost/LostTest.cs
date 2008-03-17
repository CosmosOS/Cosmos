using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lost.JIT.AMD64;
//using Cosmos.Build.Windows;

namespace Lost
{
    class LostTest
    {
        [STAThread]
        static void Main(string[] args)
        {
			var op = ProcessorInstruction.Parse("test:  ggf1: lock adc 10, 12,");
			Console.WriteLine(op);

			Console.ReadKey();
        }
    }
}
